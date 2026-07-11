# PPP Index Data

## Source

World Bank International Comparison Program (ICP).

Updated annually. Last update: 2024.

---

## PPP Conversion Factors

### Tier 1 (Multiplier: 1.0x)

Full US pricing. High-income economies.

| Country | Code | PPP Factor |
|---------|------|------------|
| United States | US | 1.00 |
| United Kingdom | GB | 1.00 |
| Canada | CA | 1.00 |
| Australia | AU | 1.00 |
| Singapore | SG | 1.00 |
| United Arab Emirates | AE | 1.00 |
| Switzerland | CH | 1.00 |
| Germany | DE | 1.00 |
| France | FR | 1.00 |
| Netherlands | NL | 1.00 |
| Sweden | SE | 1.00 |
| Denmark | DK | 1.00 |
| Ireland | IE | 1.00 |
| Norway | NO | 1.00 |
| Luxembourg | LU | 1.00 |
| Iceland | IS | 1.00 |

### Tier 2 (Multiplier: 0.75x)

Upper-middle income. Strong economies.

| Country | Code | PPP Factor |
|---------|------|------------|
| Japan | JP | 0.75 |
| South Korea | KR | 0.75 |
| New Zealand | NZ | 0.75 |
| Israel | IL | 0.75 |
| Saudi Arabia | SA | 0.75 |
| Poland | PL | 0.75 |
| Czech Republic | CZ | 0.75 |
| Hungary | HU | 0.75 |
| Portugal | PT | 0.75 |
| Greece | GR | 0.75 |
| Slovenia | SI | 0.75 |
| Slovakia | SK | 0.75 |
| Estonia | EE | 0.75 |
| Lithuania | LT | 0.75 |
| Latvia | LV | 0.75 |
| Croatia | HR | 0.75 |
| Chile | CL | 0.75 |
| Uruguay | UY | 0.75 |
| Panama | PA | 0.75 |
| Costa Rica | CR | 0.75 |

### Tier 3 (Multiplier: 0.50x)

Middle income. Emerging economies.

| Country | Code | PPP Factor |
|---------|------|------------|
| China | CN | 0.50 |
| Brazil | BR | 0.50 |
| Mexico | MX | 0.50 |
| Turkey | TR | 0.50 |
| Thailand | TH | 0.50 |
| Malaysia | MY | 0.50 |
| Argentina | AR | 0.50 |
| Colombia | CO | 0.50 |
| Peru | PE | 0.50 |
| South Africa | ZA | 0.50 |
| Romania | RO | 0.50 |
| Bulgaria | BG | 0.50 |
| Russia | RU | 0.50 |
| Kazakhstan | KZ | 0.50 |
| Ecuador | EC | 0.50 |
| Dominican Republic | DO | 0.50 |
| Guatemala | GT | 0.50 |
| Jordan | JO | 0.50 |
| Lebanon | LB | 0.50 |
| Iraq | IQ | 0.50 |

### Tier 4 (Multiplier: 0.30x)

Lower-middle income. Developing economies.

| Country | Code | PPP Factor |
|---------|------|------------|
| India | IN | 0.30 |
| Indonesia | ID | 0.30 |
| Philippines | PH | 0.30 |
| Vietnam | VN | 0.30 |
| Egypt | EG | 0.30 |
| Pakistan | PK | 0.30 |
| Bangladesh | BD | 0.30 |
| Nigeria | NG | 0.30 |
| Morocco | MA | 0.30 |
| Tunisia | TN | 0.30 |
| Kenya | KE | 0.30 |
| Ghana | GH | 0.30 |
| Senegal | SN | 0.30 |
| Nepal | NP | 0.30 |
| Sri Lanka | LK | 0.30 |
| Myanmar | MM | 0.30 |
| Cambodia | KH | 0.30 |
| Laos | LA | 0.30 |
| Bhutan | BT | 0.30 |
| Maldives | MV | 0.30 |

### Tier 5 (Multiplier: 0.15x)

Low income. Purchasing power parity adjusted.

| Country | Code | PPP Factor |
|---------|------|------------|
| Ethiopia | ET | 0.15 |
| DR Congo | CD | 0.15 |
| Tanzania | TZ | 0.15 |
| Uganda | UG | 0.15 |
| Mozambique | MZ | 0.15 |
| Madagascar | MG | 0.15 |
| Malawi | MW | 0.15 |
| Zambia | ZM | 0.15 |
| Rwanda | RW | 0.15 |
| Mali | ML | 0.15 |
| Burkina Faso | BF | 0.15 |
| Niger | NE | 0.15 |
| Chad | TD | 0.15 |
| Guinea | GN | 0.15 |
| Benin | BJ | 0.15 |
| Togo | TG | 0.15 |
| Sierra Leone | SL | 0.15 |
| Liberia | LR | 0.15 |
| Central African Republic | CF | 0.15 |
| South Sudan | SS | 0.15 |

---

## Annual Review Process

1. World Bank releases new PPP data (typically June)
2. Compare current multipliers to new data
3. Identify countries that should move tiers
4. Update multiplier table
5. Notify existing customers of changes
6. Apply new pricing to new subscriptions

### Tier Migration Rules

- Changes apply to new subscriptions immediately
- Existing subscriptions grandfathered for 12 months
- Customers notified 30 days before changes
- Option to lock current rate for 12 months

---

## Data Sources

- World Bank ICP: https://data.worldbank.org/topic/icp
- Numbeo PPP Index: https://www.numbeo.com/cost-of-living/
- IMF PPP Data: https://www.imf.org/en/Publications/WEO

---

## Currency Handling

- All prices stored in USD
- Stripe handles currency conversion
- Local currency display via Stripe Checkout
- Exchange rates updated in real-time
