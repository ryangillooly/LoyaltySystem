# Missing Customer UI Flows

This document outlines all currently missing or incomplete UI flows for the Customer Application, based on the latest UI review, documentation, and best practices. Each section includes a brief description and notes on any related backend controller endpoints that are also missing or incomplete.

---

## 1. Security & Account Management

### 1.1 2FA Backup/Recovery
- **UI Flow Missing:** No flow for generating, viewing, or using backup codes for 2FA recovery.
- **Backend Endpoint Needed:**
  - `POST /api/account/2fa/backup-codes` (generate)
  - `GET /api/account/2fa/backup-codes` (view)

### 1.2 Session Management
- **UI Flow Missing:** No way to sign out of all sessions/devices or view session history in detail.
- **Backend Endpoint Needed:**
  - `GET /api/account/sessions` (list sessions)
  - `POST /api/account/sessions/revoke-all` (sign out everywhere)

### 1.3 Account Recovery
- **UI Flow Missing:** No flow for account recovery if 2FA device is lost.
- **Backend Endpoint Needed:**
  - `POST /api/account/2fa/recovery-request`

---

## 2. Data Privacy & Export

### 2.1 Data Export Status
- **UI Flow Missing:** No progress indicator or download link for data export requests.
- **Backend Endpoint Needed:**
  - `GET /api/privacy/export/status` (check status)
  - `GET /api/privacy/export/download` (download data)

---

## 3. Notifications & Preferences

### 3.1 Granular Notification Settings
- **UI Flow Missing:** No per-event or per-channel notification controls (e.g., enable/disable for each type).
- **Backend Endpoint Needed:**
  - `PATCH /api/notifications/preferences` (update granular preferences)

### 3.2 Push Notification Opt-in
- **UI Flow Missing:** No explicit opt-in/out prompt for push notifications.
- **Backend Endpoint Needed:**
  - `POST /api/notifications/push/opt-in`
  - `POST /api/notifications/push/opt-out`

---

## 4. Support & Help

### 4.1 Help Center/Contact Support
- **UI Flow Missing:** No in-app help, FAQ, or contact support flow.
- **Backend Endpoint Needed:**
  - `GET /api/support/faqs`
  - `POST /api/support/contact`

---

## 5. Accessibility

### 5.1 Accessibility Settings
- **UI Flow Missing:** No settings for font size, contrast, or screen reader support.
- **Backend Endpoint Needed:**
  - `PATCH /api/account/accessibility` (save preferences)

---

## 6. Transaction History

### 6.1 In-App Transaction History
- **UI Flow Missing:** No in-app view of transaction history (only available via export).
- **Backend Endpoint Needed:**
  - `GET /api/transactions` (list transactions)

---

## 7. Reward Discovery & Sharing

### 7.1 Reward Search/Filter
- **UI Flow Missing:** No search or filter for rewards.
- **Backend Endpoint Needed:**
  - `GET /api/rewards/search?query=...`

### 7.2 Reward Sharing
- **UI Flow Missing:** No way to share rewards with others.
- **Backend Endpoint Needed:**
  - `POST /api/rewards/share`

---

## 8. Referrals

### 8.1 Referral Invites
- **UI Flow Missing:** No flow to invite friends via SMS/email/social (only code display).
- **Backend Endpoint Needed:**
  - `POST /api/referrals/invite`

---

## 9. Onboarding/Tutorial

### 9.1 First-Time User Onboarding
- **UI Flow Missing:** No onboarding or tutorial for new users.
- **Backend Endpoint Needed:**
  - (Typically handled client-side, but could log onboarding completion: `POST /api/account/onboarding-complete`)

---

## 10. App Updates

### 10.1 Update Prompt
- **UI Flow Missing:** No prompt or flow for app updates or changelog.
- **Backend Endpoint Needed:**
  - `GET /api/app/version` (check for updates)

---

# Summary Table

| Area                | Missing UI Flow                | Missing Backend Endpoint(s)                |
|---------------------|-------------------------------|--------------------------------------------|
| Security            | 2FA backup/recovery           | /api/account/2fa/backup-codes, /recovery   |
| Security            | Session management            | /api/account/sessions                      |
| Data Privacy        | Export status/download        | /api/privacy/export/status, /download       |
| Notifications       | Granular controls, opt-in     | /api/notifications/preferences, /push      |
| Support             | Help/contact                  | /api/support/faqs, /contact                |
| Accessibility       | Accessibility settings        | /api/account/accessibility                 |
| Transactions        | In-app history                | /api/transactions                          |
| Rewards             | Search/filter, sharing        | /api/rewards/search, /share                 |
| Referrals           | Invite flow                   | /api/referrals/invite                      |
| Onboarding          | Tutorial/onboarding           | /api/account/onboarding-complete           |
| App Updates         | Update prompt                 | /api/app/version                           |

---

*Last updated: 2025-05-12* 