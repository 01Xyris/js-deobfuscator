public static class RegexPatterns
{
    public const string SPLIT_JOIN_PATTERN = @"(?<=\.split\()\w+(?=\)\.join\(""""\))";
    public const string VARIABLE_PATTERN = @"var {0} = ""(.+?)""";

    public const string LONG_RANDOM_STRING_PATTERN = @"^\s*(var\s+)?[a-zA-Z_$][a-zA-Z0-9_$]{50,}\s*=\s*(?:""[a-zA-Z0-9_$]{50,}""|'[a-zA-Z0-9_$]{50,}'|[a-zA-Z0-9_$]{50,})\s*;?\s*$";

    public const string PLUS_PATTERN = @"(?<=\+ )[a-zA-Z]+(?= \+)";
   
    public const string A_PATTERN = @"'([^']+?)','A'";
    public const string EMPTY_LINES_PATTERN = @"^\s*$\n|\r";
    public const string URL_PAIRS_PATTERN = @"(\w+)\s+([^;]+)";
    public const string PAYLOAD_URL_PATTERN = @"(&[^']+txt\.[^/]+/\d+/\d+/stnemhcatta/moc\.ppadrocsid\.ndc//:sptth)";

    public const string PAYLOAD_URL_PATTERN3008 = @"(txt\.[^/]+/[^/]+/moc\.[^/]+//:sptth)";
    
    
    public const string VBS_PLUS_PATTERN = @"(?<=\& )[a-zA-Z]+(?= \&)";
    public const string VBS_VARIABLE_PATTERN = @"{0} = ""(.+?)""";
    public const string VBS_JOIN = @"(?<=, )(\w+)(?=, """")";
    public const string VBS_VARIABLE_PATTERN2 = @"(?<=StrReverse\("")[^""]+(?=""\))";

} 