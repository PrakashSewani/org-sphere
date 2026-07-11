# Authentication

## Authentication Mechanisms

OrgSphere supports multiple authentication methods for different use cases.

---

## Authentication Methods

### Email/Password

- Email-based registration
- Password requirements
- Password reset flow
- Account lockout

### Single Sign-On (SSO)

- SAML 2.0
- OpenID Connect (OIDC)
- OAuth 2.0

### Multi-Factor Authentication (MFA)

- TOTP (Google Authenticator, Authy)
- SMS verification
- Email verification
- Hardware tokens (YubiKey)

---

## JWT Tokens

### Access Token

```json
{
  "sub": "user-123",
  "tenantId": "tenant-456",
  "email": "john@company.com",
  "roles": ["employee", "manager"],
  "iat": 1705312200,
  "exp": 1705315800
}
```

### Refresh Token

- Longer expiration (30 days)
- One-time use
- Rotation on use
- Revocation capability

### Token Lifecycle

```
Login → Access Token (15 min) + Refresh Token (30 days)
                                    ↓
                              Use Refresh Token
                                    ↓
                              New Access Token
                              New Refresh Token
```

---

## Password Policies

### Requirements

- Minimum 12 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 number
- At least 1 special character
- No common passwords
- No personal information

### History

- Remember last 12 passwords
- Prevent password reuse
- Minimum age: 24 hours

### Expiration

- Maximum age: 90 days
- Warning at 7 days
- Force change on expiration

---

## Account Security

### Lockout

- Lock after 5 failed attempts
- Lock duration: 30 minutes
- Manual unlock by admin
- Notification on lockout

### Session Management

- Concurrent session limit: 5
- Session timeout: 30 minutes
- Device tracking
- Session revocation

---

## SSO Configuration

### SAML 2.0

```json
{
  "provider": "okta",
  "protocol": "saml",
  "entityId": "https://orgsphere.com",
  "ssoUrl": "https://company.okta.com/sso",
  "sloUrl": "https://company.okta.com/slo",
  "certificate": "-----BEGIN CERTIFICATE-----..."
}
```

### OIDC

```json
{
  "provider": "google",
  "protocol": "oidc",
  "clientId": "abc123",
  "clientSecret": "secret",
  "discoveryUrl": "https://accounts.google.com/.well-known/openid-configuration"
}
```

---

## MFA Setup

### User Flow

1. User enables MFA in settings
2. System generates secret
3. User scans QR code
4. User enters verification code
5. System confirms setup
6. Recovery codes provided

### Recovery Codes

- 10 single-use codes
- Stored encrypted
- Displayed once
- Regeneration available

---

## API Authentication

### API Keys

- Tenant-scoped
- Scoped permissions
- Rotatable
- Rate limited

### OAuth 2.0

- Authorization code flow
- Client credentials flow
- PKCE for SPA

---

## Session Security

### Cookie Settings

```
HttpOnly: true
Secure: true
SameSite: Strict
Path: /
Max-Age: 1800
```

### Token Storage

- Access token: Memory only
- Refresh token: HttpOnly cookie
- MFA token: Session storage

---

## Audit Logging

### Logged Events

- Login attempts (success/failure)
- Password changes
- MFA changes
- SSO events
- Session creation/termination
- API key usage

### Log Format

```json
{
  "event": "login.success",
  "userId": "user-123",
  "tenantId": "tenant-456",
  "ip": "192.168.1.1",
  "userAgent": "...",
  "timestamp": "2025-01-15T10:30:00Z"
}
```
