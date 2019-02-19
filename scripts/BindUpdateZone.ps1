Set-Location C:\named\zonewriter\
& dotnet C:\named\zonewriter\Ddi.Registry.ZoneWriter.dll
if($LastExitCode -eq 100) { exit }

& Icacls c:\named\etc  /grant named:F

$sout = & c:\named\bin\named-checkzone.exe registry.ddialliance.org C:\named\etc\registry.ddialliance.org.zone
if($LastExitCode -ne 0) 
{
    $smtpServer = "jessica.algenta.com"
    
    $mail = new-object System.Net.Mail.MailMessage
    $mail.From = new-object System.Net.Mail.MailAddress("nagios@jessica.algenta.com")
    $mail.To.Add("dan@algenta.com")
    $mail.To.Add("jeremy@algenta.com")
    $mail.Subject = "Invalid DDI registry zone file"
    $mail.Body = "Invalid DDI registry zone file " + $sout
    
    $smtp = new-object Net.Mail.SmtpClient($smtpServer)
    $smtp.Send($mail)
    exit
}

& c:\named\bin\rndc.exe reload