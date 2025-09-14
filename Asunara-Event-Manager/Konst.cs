namespace EventManager;

public static class Konst
{
    public const string ButtonReminderYes = "button-reminder-yes";
    public const string ButtonReminderNo = "button-reminder-no";
    
    public const string ButtonFeedbackYes = "button-feedback-yes";
    public const string ButtonFeedbackNo = "button-feedback-no";
    
    public const string ButtonFeedbackStarGroup = "button-feedback-star";
    
    public const string ButtonFeedback1Star = "button-feedback-star-1";
    public const string ButtonFeedback2Star = "button-feedback-star-2";
    public const string ButtonFeedback3Star = "button-feedback-star-3";
    public const string ButtonFeedback4Star = "button-feedback-star-4";
    public const string ButtonFeedback5Star = "button-feedback-star-5";
    
    public const string ButtonFeedbackVisibilityGroup = "button-feedback-visibility-";
    public const string ButtonFeedbackVisibilityAnonymous = "button-feedback-visibility-anonymous";
    public const string ButtonFeedbackVisibilityPublic = "button-feedback-visibility-public";
    
    public const string ButtonMoreFeedback = "button-more-feedback";
    
    public const string ButtonBirthdayGroup = "button-birthday-";
    public const string ButtonBirthdayRegister = "button-birthday-register";
    public const string ButtonBirthdayDelete = "button-birthday-delete";

    public const string PayloadDelimiter = "||";


    public static class Modal
    {
        public static class Feedback
        {
            public const string Id = "modal-feedback";
            
            public const string GoodInputId = "modal-input-feedback-good";
            public const string CriticInputId = "modal-input-feedback-critic";
            public const string SuggestionInputId = "modal-input-feedback-suggestion";
        }

        public static class Birthday
        {
            public const string Id = "modal-birthday";
            
            public const string DayInputId = "modal-input-birthday-date-day";
            public const string MonthInputId = "modal-input-birthday-date-month";
            public const string YearInputId = "modal-input-birthday-date-year";
        }
    }
}