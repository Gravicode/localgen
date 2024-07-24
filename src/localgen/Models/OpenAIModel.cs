namespace localgen.Models
{
    #region Models
    public class CompletionRequest
    {
        public string model { get; set; }
        public CompletionMessage[] messages { get; set; }
        public float temperature { get; set; }
    }

    public class CompletionMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }


    public class CompletionResponse
    {
        public string id { get; set; }
        public string _object { get; set; } = "chat.completion";
        public long created { get; set; } = DateTime.Now.Ticks;
        public string model { get; set; }
        public CompletionUsage usage { get; set; } = new();
        public CompletionChoice[] choices { get; set; }
    }

    public class CompletionUsage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }

    public class CompletionChoice
    {
        public CompletionMessage message { get; set; }
        public object logprobs { get; set; } = null;
        public string finish_reason { get; set; }
        public int index { get; set; }
    }


    #endregion

}
