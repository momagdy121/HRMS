namespace HRSystem.Business.Helpers;

public static class EmailTemplates
{
    public static string PasswordResetEmail(string employeeName, string resetUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Password Reset - HRMS Portal</title>
</head>
<body style=""margin: 0; padding: 0; background-color: #f4f6f9; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;"">
    <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""background-color: #f4f6f9; padding: 40px 20px;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""600"" style=""max-width: 600px; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 2px 12px rgba(0,0,0,0.08);"">
                    <!-- Header -->
                    <tr>
                        <td style=""background: linear-gradient(135deg, #1a237e 0%, #283593 100%); padding: 32px 40px; text-align: center;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 24px; font-weight: 600; letter-spacing: -0.5px;"">HRMS Portal</h1>
                        </td>
                    </tr>
                    <!-- Body -->
                    <tr>
                        <td style=""padding: 40px;"">
                            <h2 style=""color: #1a237e; margin: 0 0 16px; font-size: 20px; font-weight: 600;"">Password Reset Request</h2>
                            <p style=""color: #455a64; font-size: 15px; line-height: 1.6; margin: 0 0 24px;"">
                                Hi {System.Net.WebUtility.HtmlEncode(employeeName)},
                            </p>
                            <p style=""color: #455a64; font-size: 15px; line-height: 1.6; margin: 0 0 32px;"">
                                We received a request to reset the password for your HRMS Portal account. Click the button below to set a new password. This link will expire for security reasons.
                            </p>
                            <!-- CTA Button -->
                            <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" width=""100%"">
                                <tr>
                                    <td align=""center"">
                                        <a href=""{resetUrl}"" style=""display: inline-block; background: linear-gradient(135deg, #1a237e 0%, #283593 100%); color: #ffffff; text-decoration: none; padding: 14px 40px; border-radius: 8px; font-size: 15px; font-weight: 600; letter-spacing: 0.3px;"">
                                            Reset My Password
                                        </a>
                                    </td>
                                </tr>
                            </table>
                            <p style=""color: #78909c; font-size: 13px; line-height: 1.6; margin: 32px 0 0; padding-top: 24px; border-top: 1px solid #eceff1;"">
                                If you didn't request a password reset, you can safely ignore this email. Your password will remain unchanged.
                            </p>
                            <p style=""color: #90a4ae; font-size: 12px; line-height: 1.5; margin: 16px 0 0;"">
                                If the button doesn't work, copy and paste this link into your browser:<br />
                                <a href=""{resetUrl}"" style=""color: #1a237e; word-break: break-all;"">{resetUrl}</a>
                            </p>
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style=""background-color: #f8f9fa; padding: 24px 40px; text-align: center; border-top: 1px solid #eceff1;"">
                            <p style=""color: #90a4ae; font-size: 12px; margin: 0;"">
                                &copy; {DateTime.Now.Year} HRMS Portal. All rights reserved.
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}
