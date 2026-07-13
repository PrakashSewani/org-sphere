# Billing Architecture

## Design Principle

The billing/payment system is **completely separated** from the OrgSphere core application. OrgSphere has zero billing dependencies. This enables two deployment models:

1. **SaaS (Cloud)** — Subscription billing handled externally
2. **Self-Hosted** — One-time license purchase, validated offline

---

## Deployment Models

```
┌─────────────────────────────────────────────────────────┐
│                 OrgSphere Core App                       │
│  (No billing code, no Stripe SDK, no payment logic)     │
│                                                         │
│  Only knows: "Is this tenant licensed?"                 │
└──────────────────────┬──────────────────────────────────┘
                       │
          ┌────────────┴────────────┐
          │                         │
    ┌─────▼──────┐          ┌──────▼───────┐
    │   SaaS     │          │  Self-Hosted  │
    │ Deployment │          │  Deployment   │
    │            │          │              │
    │ Validates  │          │ Validates    │
    │ JWT token  │          │ license key  │
    │ from auth  │          │ offline with │
    │ service    │          │ embedded key │
    └─────┬──────┘          └──────────────┘
          │
    ┌─────▼──────┐
    │  Separate  │
    │  Billing   │  ← Own repo: OrgSphere.Billing
    │  Service   │  ← Own deployment
    │            │  ← Stripe integration
    └────────────┘
```

---

## OrgSphere Core (No Billing Code)

The main OrgSphere app has a single interface for license validation:

```csharp
// In OrgSphere.Domain
public interface ILicenseValidator
{
    Task<LicenseStatus> ValidateAsync(TenantId tenantId, CancellationToken ct = default);
}

public record LicenseStatus
{
    public bool IsValid { get; init; }
    public string Tier { get; init; } = string.Empty;
    public int MaxMembers { get; init; }
    public DateTime? ExpiresAt { get; init; }
}
```

### SaaS Deployment

```csharp
// JWT token contains license info, validated at auth time
public class JwtLicenseValidator : ILicenseValidator
{
    public Task<LicenseStatus> ValidateAsync(TenantId tenantId, CancellationToken ct)
    {
        // License is already validated via JWT at login
        // Just check token claims
        return Task.FromResult(new LicenseStatus
        {
            IsValid = true,
            Tier = "growth",
            MaxMembers = 200
        });
    }
}
```

### Self-Hosted Deployment

```csharp
// License key validated offline with embedded public key
public class OfflineLicenseValidator : ILicenseValidator
{
    private readonly LicenseKeyStore _store;

    public Task<LicenseStatus> ValidateAsync(TenantId tenantId, CancellationToken ct)
    {
        var license = _store.GetLicense();
        if (license is null)
        {
            return Task.FromResult(new LicenseStatus { IsValid = false });
        }

        // Verify signature with embedded public key
        var isValid = LicenseCrypto.Verify(license.Key, license.Signature, _publicKey);

        return Task.FromResult(new LicenseStatus
        {
            IsValid = isValid,
            Tier = license.Tier,
            MaxMembers = license.MaxMembers,
            ExpiresAt = license.ExpiresAt
        });
    }
}
```

---

## Separate Billing Service

The billing service is an independent application:

```
OrgSphere.Billing/           ← Separate repo
├── Controllers/
│   ├── StripeWebhookController.cs
│   └── LicenseController.cs
├── Services/
│   ├── StripeService.cs
│   ├── LicenseGenerator.cs
│   └── SubscriptionManager.cs
├── Entities/
│   ├── Subscription.cs
│   ├── License.cs
│   └── Invoice.cs
└── Program.cs
```

### Responsibilities

| OrgSphere Core | Billing Service |
|----------------|-----------------|
| Org management | Stripe integration |
| Employee hierarchy | Subscription lifecycle |
| Graph operations | License key generation |
| Real-time updates | Payment processing |
| User auth | Invoice management |
| **No billing code** | **No org logic** |

---

## License Key System

### Key Format

```
ORG-XXXX-XXXX-XXXX-XXXX
```

### Key Structure (Encoded)

```
{
  "tenantId": "uuid",
  "tier": "starter|growth|business|enterprise",
  "maxMembers": 50,
  "features": ["ai", "recruitment"],
  "issuedAt": "2025-01-15T00:00:00Z",
  "expiresAt": null,  // null = perpetual
  "signature": "..."
}
```

### Offline Validation

1. License key is embedded in app configuration at purchase time
2. Public key for verification is compiled into the app
3. No network call required for validation
4. App decodes the key, verifies signature locally
5. Tier limits enforced in-memory

```csharp
// Embedded in app at build/deploy time
public static class LicenseCrypto
{
    private const string PublicKey = "MIIBIjANBgkq..."; // RSA public key

    public static bool Verify(string key, byte[] signature, string? overrideKey = null)
    {
        var data = Encoding.UTF8.GetBytes(key);
        using var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(overrideKey ?? PublicKey), out _);
        return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}
```

---

## Pricing

### Cloud SaaS (Monthly Subscription)

| Plan | Price | Members |
|------|-------|---------|
| Starter | $49/mo | 11-50 |
| Growth | $149/mo | 51-200 |
| Business | $399/mo | 201-1000 |
| Enterprise | $899/mo | 1001+ |

### Self-Hosted (One-Time License)

| Tier | Price | Members |
|------|-------|---------|
| Starter | $999 | 11-50 |
| Business | $2,999 | 201-1000 |
| Enterprise | $7,999 | 1001+ |

### Add-Ons (SaaS only)

| Add-On | Price |
|--------|-------|
| AI Assistant | $49/mo |
| Recruitment | $79/mo |
| Performance | $59/mo |
| Analytics Pro | $39/mo |

---

## PPP (Purchasing Power Parity)

Regional pricing applied by the billing service, not the core app:

| Tier | Countries | Multiplier |
|------|-----------|------------|
| 1 | US, GB, CA, AU, SG, AE, CH, DE, FR | 1.0x |
| 2 | JP, KR, NZ, IL, SA, PL | 0.75x |
| 3 | CN, BR, MX, TR, TH, MY | 0.50x |
| 4 | IN, ID, PH, VN, EG, PK | 0.30x |
| 5 | NG, KE, GH, NP, LK | 0.15x |

---

## Webhook Integration (Billing Service Only)

Stripe webhooks are handled exclusively by the billing service:

| Event | Action |
|-------|--------|
| customer.subscription.created | Activate SaaS subscription |
| customer.subscription.updated | Sync plan changes |
| customer.subscription.deleted | Deactivate subscription |
| invoice.paid | Confirm payment |
| invoice.payment_failed | Trigger dunning |
| checkout.session.completed | Complete signup |

---

## Dunning & Recovery (SaaS Only)

```
Day 0:  Payment fails → Stripe retries → Email admin
Day 3:  Second retry → Warning banner in app
Day 7:  Third retry → Feature restrictions
Day 14: Final retry → Subscription suspended (data retained 30 days)
Day 44: Data deleted
```

---

## Tax Handling

Handled by the billing service via Stripe Tax:

- US state sales tax
- EU VAT
- UK VAT
- GST (AU, CA, IN)
