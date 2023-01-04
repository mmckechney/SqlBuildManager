namespace SqlSync.SqlBuild.Validator
{
    public class ScriptSettingValidation
    {
        public static ScriptTimeoutValidationResult CheckScriptTimeoutValue(string input, int defaultMinumumValue)
        {
            int timeOut;
            if (int.TryParse(input, out timeOut))
            {
                if (timeOut < defaultMinumumValue)
                {
                    return ScriptTimeoutValidationResult.TimeOutTooSmall;
                }
            }
            else
            {
                return ScriptTimeoutValidationResult.NonIntegerValue;
            }
            return ScriptTimeoutValidationResult.Ok;

        }
    }

    public enum ScriptTimeoutValidationResult
    {
        Ok,
        TimeOutTooSmall,
        NonIntegerValue

    }
}
