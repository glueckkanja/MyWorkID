﻿namespace MyWorkID.Server.Options
{
    /// <summary>
    /// Authentication context ids for the application functions.
    /// </summary>
    public class AppFunctionsOptions
    {
        public string? DismissUserRisk { get; set; }
        public string? GenerateTap { get; set; }
        public string? ResetPassword { get; set; }
    }
}
