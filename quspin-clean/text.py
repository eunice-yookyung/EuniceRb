#!/usr/bin/env python3
"""
test_sms.py — Minimal SMS send test.

Pick ONE method below by setting METHOD = "email" or "twilio",
fill in the CONFIG values, then run:  python test_sms.py
"""

METHOD = "email"   # "email" (free carrier gateway) or "twilio" (paid, reliable)

# ---------- EMAIL-TO-SMS CONFIG ----------
EMAIL_FROM     = "eunyk124@gmail.com"
EMAIL_PASSWORD = "IwbiSK,amtAwIw2.5"        # Gmail App Password (needs 2FA), NOT your login
SMS_TO         = "3392226564@vtext.com"     # phone@carrier-gateway
SMTP_SERVER    = "smtp.gmail.com"
SMTP_PORT      = 465

# ---------- TWILIO CONFIG ----------
TWILIO_SID   = "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
TWILIO_TOKEN = "your_auth_token"
TWILIO_FROM  = "+1YOUR_TWILIO_NUMBER"
TWILIO_TO    = "+15551234567"

MESSAGE = "Test from MATLAB-replacement Python script. If you got this, SMS works."


def send_via_email():
    import smtplib
    from email.message import EmailMessage

    msg = EmailMessage()
    msg["From"] = EMAIL_FROM
    msg["To"] = SMS_TO
    msg["Subject"] = "SMS test"
    msg.set_content(MESSAGE)

    with smtplib.SMTP_SSL(SMTP_SERVER, SMTP_PORT) as s:
        s.login(EMAIL_FROM, EMAIL_PASSWORD)
        s.send_message(msg)
    print(f"Email handed off to {SMTP_SERVER} for {SMS_TO}.")
    print("Note: a successful send here only means the SMTP server accepted it.")
    print("If no text arrives in ~1 min, the carrier gateway likely dropped it.")


def send_via_twilio():
    from twilio.rest import Client  # pip install twilio

    client = Client(TWILIO_SID, TWILIO_TOKEN)
    m = client.messages.create(body=MESSAGE, from_=TWILIO_FROM, to=TWILIO_TO)
    print(f"Twilio accepted message. SID: {m.sid}, status: {m.status}")


def main():
    print(f"Sending test SMS via '{METHOD}'...")
    try:
        if METHOD == "email":
            send_via_email()
        elif METHOD == "twilio":
            send_via_twilio()
        else:
            print(f"Unknown METHOD: {METHOD!r}. Use 'email' or 'twilio'.")
            return
        print("Done — check your phone.")
    except Exception as e:
        print(f"FAILED: {type(e).__name__}: {e}")
        print("\nCommon causes:")
        if METHOD == "email":
            print("  - Wrong password: Gmail needs an App Password (2FA on), not your login.")
            print("  - Wrong carrier gateway domain for SMS_TO.")
            print("  - Network/firewall blocking port 465.")
        else:
            print("  - Wrong SID/token, or 'twilio' not installed (pip install twilio).")
            print("  - 'to' number not verified (required on Twilio trial accounts).")


if __name__ == "__main__":
    main()