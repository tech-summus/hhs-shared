namespace Hhs.Shared.Helper.Utils;

public static class StringOperations
{
    public static string ReplaceInvalidChars(string text, bool isEmail = false, string replaceInvalidChar = "")
    {
        var result = string.Empty;
        if (string.IsNullOrWhiteSpace(text)) return result;

        var upperText = text.ToUpper();
        for (var i = 0; i < text.Length; i++)
        {
            var charCode = (byte)upperText[i];
            if (
                charCode is > 64 and < 91 // A-Z
                || charCode is > 47 and < 58 // 0-9
                || charCode is 45 or 46 or 95 // - . _
            )
            {
                result += text[i];
                continue;
            }

            if (isEmail && charCode is 43  or 64) // + @
            {
                result += text[i];
                continue;
            }

            result += replaceInvalidChar;
        }

        return result;
    }

    public static string FirstCharCapitalize(string text, string[] defaultDelimeters = null)
    {
        var capitalizeRoleName = string.Empty;
        if (string.IsNullOrWhiteSpace(text)) return capitalizeRoleName;
        defaultDelimeters ??= new[] { "-", "_" };

        var clearedText = text.ToUpper();
        clearedText = defaultDelimeters.Aggregate(clearedText, (current, delimeter) => current.Replace(delimeter, " "));

        foreach (var item in clearedText.Split(" "))
        {
            switch (item.Length)
            {
                case 0: continue;
                case 1:
                    capitalizeRoleName += item[0];
                    break;
                default:
                    capitalizeRoleName += string.Concat(item[0], item.Substring(1).ToLower());
                    break;
            }

            capitalizeRoleName += " ";
        }

        return capitalizeRoleName.Trim();
    }

    public static string Base64Encode(string plainText)
    {
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        return System.Convert.ToBase64String(plainTextBytes);
    }

    public static string Base64Decode(string base64EncodedData)
    {
        var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }
}