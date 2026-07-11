# Billing Architecture

## Stripe Integration

OrgSphere uses Stripe for payment processing across both Cloud SaaS and self-hosted license sales.

---

## Stripe Products

### Cloud SaaS Subscriptions

```
orgsphere-starter     → $49/month    (11-50 members)
orgsphere-growth      → $149/month   (51-200 members)
orgsphere-business    → $399/month   (201-1000 members)
orgsphere-enterprise  → $899/month   (1001+ members)
```

### Self-Hosted Licenses (One-Time)

```
license-starter       → $999 one-time
license-business      → $2,999 one-time
license-enterprise    → $7,999 one-time
```

### Add-Ons

```
addon-ai              → $49/month
addon-recruitment     → $79/month
addon-performance     → $59/month
addon-analytics-pro   → $39/month
addon-storage         → $9/month
```

### Support Plans

```
support-priority      → $499/year
support-premium       → $2,999/year
```

---

## Subscription Lifecycle

### Cloud SaaS Flow

```
┌─────────────────────────────────────────────────────────┐
│                    Subscription Flow                     │
│                                                         │
│  1. User selects plan                                   │
│  2. Create Stripe Customer                              │
│  3. Create Subscription (trial)                         │
│  4. 14-day trial                                        │
│  5. Convert to paid                                     │
│  6. Monthly billing                                     │
│  7. Upgrade/downgrade as needed                         │
└─────────────────────────────────────────────────────────┘
```

### Trial Period

- 14 days free on all paid plans
- No credit card required
- Full feature access
- Automatic conversion if card on file

---

## Usage Reporting

### Member Count Tracking

OrgSphere tracks active member count and reports to Stripe at billing cycle end.

```
orgsphere.growth.members Usage Record
{
  quantity: 75,          // active members at cycle end
  timestamp: 1705312200
}
```

### Proration

- Members added mid-cycle: Prorated charge
- Members removed mid-cycle: Prorated credit
- Plan upgrade: Immediate proration
- Plan downgrade: Effective next cycle

---

## Webhook Integration

### Stripe Webhooks

| Event | Action |
|-------|--------|
| customer.subscription.created | Activate subscription |
| customer.subscription.updated | Sync plan changes |
| customer.subscription.deleted | Deactivate subscription |
| invoice.paid | Confirm payment |
| invoice.payment_failed | Trigger dunning |
| checkout.session.completed | Complete signup |

### Webhook Security

- Verify Stripe signatures
- Idempotent processing
- Retry with exponential backoff
- Dead letter queue

---

## Dunning & Recovery

### Payment Failure Flow

```
Day 0: Payment fails
  → Stripe Smart Retries
  → Email notification to admin

Day 3: Second retry
  → Email notification
  → Warning banner in app

Day 7: Third retry
  → Email notification
  → Feature restrictions

Day 14: Final retry
  → Subscription suspended
  → Data retained for 30 days
  → Final email with reactivation link

Day 44: Data deleted
  → Permanent deletion
  → Final notice email
```

### Smart Retries

Stripe automatically retries failed payments using machine learning to optimize retry timing.

---

## Self-Hosted License Flow

### Purchase Flow

```
1. User selects license tier
2. Create Stripe one-time payment
3. Process payment
4. Generate license key
5. Send license key via email
6. User downloads software
7. User activates with license key
```

### License Key Format

```
ORG-XXXX-XXXX-XXXX-XXXX
```

### License Validation

- Validate key against Stripe payment
- Check tier limits
- Verify deployment domain
- Periodic online check (optional)

---

## Checkout Integration

### Stripe Checkout

Hosted checkout page for:

- Subscription signup
- One-time license purchase
- Add-on purchases
- Plan upgrades

### Custom Checkout

Embedded checkout for:

- In-app upgrades
- Seamless experience
- Brand consistency

---

## Customer Portal

### Stripe Customer Portal

Self-service portal for:

- Update payment method
- View invoices
- Download receipts
- Cancel subscription
- Update billing info

### In-App Portal

Embedded billing management:

- Current plan display
- Usage metrics
- Upgrade/downgrade buttons
- Invoice history

---

## Regional Pricing (PPP)

### Purchasing Power Parity

Prices adjusted based on World Bank PPP index data. Updated annually.

### PPP Index Data Source

```typescript
// Simplified PPP multipliers (base: US = 1.0)
const PPP_MULTIPLIERS = {
  // Tier 1: Full price
  US: 1.0, GB: 1.0, CA: 1.0, AU: 1.0,
  SG: 1.0, AE: 1.0, CH: 1.0,
  DE: 1.0, FR: 1.0, NL: 1.0, SE: 1.0,

  // Tier 2: 0.75x
  JP: 0.75, KR: 0.75, NZ: 0.75, IL: 0.75,
  SA: 0.75, PL: 0.75, CZ: 0.75,

  // Tier 3: 0.50x
  CN: 0.50, BR: 0.50, MX: 0.50, TR: 0.50,
  TH: 0.50, MY: 0.50, AR: 0.50, CO: 0.50,

  // Tier 4: 0.30x
  IN: 0.30, ID: 0.30, PH: 0.30, VN: 0.30,
  EG: 0.30, PK: 0.30, BD: 0.30,

  // Tier 5: 0.15x
  NG: 0.15, KE: 0.15, GH: 0.15, SN: 0.15,
  NP: 0.15, LK: 0.15, MM: 0.15,
};
```

### Price Calculation

```typescript
function getRegionalPrice(
  basePrice: number,
  countryCode: string
): number {
  const multiplier = PPP_MULTIPLIERS[countryCode] ?? 1.0;
  const adjusted = basePrice * multiplier;
  return Math.round(adjusted); // Round to whole number
}

// Example: Growth plan
getRegionalPrice(149, 'US');   // $149
getRegionalPrice(149, 'IN');   // $45
getRegionalPrice(149, 'NG');   // $22
```

### Stripe Price Mapping

Create multiple Stripe Prices per product:

```
orgsphere-starter-tier1     → $49/month
orgsphere-starter-tier2     → $37/month
orgsphere-starter-tier3     → $25/month
orgsphere-starter-tier4     → $15/month
orgsphere-starter-tier5     → $7/month
```

### Customer Region Detection

```typescript
async function detectCustomerRegion(
  customerId: string
): Promise<string> {
  const customer = await stripe.customers.retrieve(customerId);
  const country = customer.address?.country
    ?? customer.invoice_settings?.custom_fields?.find(
      f => f.name === 'country'
    )?.value;

  return country ?? 'US'; // Default to Tier 1
}
```

### Region Override

Customers can override auto-detected region:

1. Provide business registration
2. Verify billing address
3. Manual review by support
4. Update region in Stripe metadata

### Anti-Abuse Rules

| Rule | Action |
|------|--------|
| VPN detected | Flag for review |
| Payment method mismatch | Block tier change |
| Rapid region changes | Manual review |
| Suspicious patterns | Account investigation |

---

## Tax Handling

### Stripe Tax

Automatic tax calculation based on:

- Customer location
- Product type
- Tax jurisdiction

### Supported Taxes

- US state sales tax
- EU VAT
- UK VAT
- GST (Australia, Canada, India)
- Custom tax rules

---

## Revenue Recognition

### Deferred Revenue

- Annual subscriptions: Recognize monthly
- Self-hosted licenses: Recognize at delivery
- Support plans: Recognize monthly

### Reporting

- MRR (Monthly Recurring Revenue)
- ARR (Annual Recurring Revenue)
- Churn rate
- Expansion revenue
- LTV (Lifetime Value)
- CAC (Customer Acquisition Cost)

---

## Implementation

### Stripe SDK

```typescript
import Stripe from 'stripe';

const stripe = new Stripe(process.env.STRIPE_SECRET_KEY);

// Create subscription
const subscription = await stripe.subscriptions.create({
  customer: customerId,
  items: [{ price: planPriceId }],
  trial_period_days: 14,
  payment_behavior: 'default_incomplete',
  expand: ['latest_invoice.payment_intent'],
});

// Report usage
await stripe.subscriptionItems.createUsageRecord(
  subscriptionItem.id,
  {
    quantity: memberCount,
    timestamp: Math.floor(Date.now() / 1000),
    action: 'set',
  }
);
```

### Webhook Handler

```typescript
app.post('/webhooks/stripe', express.raw({type: 'application/json'}), async (req, res) => {
  const sig = req.headers['stripe-signature'];
  const event = stripe.webhooks.constructEvent(req.body, sig, endpointSecret);

  switch (event.type) {
    case 'invoice.paid':
      await handleInvoicePaid(event.data.object);
      break;
    case 'invoice.payment_failed':
      await handlePaymentFailed(event.data.object);
      break;
    // ... other events
  }

  res.json({ received: true });
});
```

---

## Database Schema

### Subscription Table

```sql
CREATE TABLE subscriptions (
  id UUID PRIMARY KEY,
  tenant_id UUID NOT NULL,
  stripe_subscription_id VARCHAR(255),
  stripe_customer_id VARCHAR(255),
  plan VARCHAR(50) NOT NULL,
  status VARCHAR(20) NOT NULL,
  current_period_start TIMESTAMP,
  current_period_end TIMESTAMP,
  trial_end TIMESTAMP,
  created_at TIMESTAMP DEFAULT NOW(),
  updated_at TIMESTAMP DEFAULT NOW()
);
```

### License Table

```sql
CREATE TABLE licenses (
  id UUID PRIMARY KEY,
  license_key VARCHAR(255) UNIQUE NOT NULL,
  stripe_payment_id VARCHAR(255),
  tier VARCHAR(50) NOT NULL,
  max_members INTEGER,
  customer_email VARCHAR(255),
  customer_name VARCHAR(255),
  activated_at TIMESTAMP,
  expires_at TIMESTAMP,
  created_at TIMESTAMP DEFAULT NOW()
);
```

### Invoice Table

```sql
CREATE TABLE invoices (
  id UUID PRIMARY KEY,
  tenant_id UUID NOT NULL,
  stripe_invoice_id VARCHAR(255),
  amount INTEGER NOT NULL,
  currency VARCHAR(3) DEFAULT 'usd',
  status VARCHAR(20) NOT NULL,
  invoice_url VARCHAR(500),
  created_at TIMESTAMP DEFAULT NOW()
);
```
