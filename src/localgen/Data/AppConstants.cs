using localgen.Models;

namespace localgen.Data;
public class AppConstants
{
    public const string ModelFolderName = "AIModels";
    public static readonly string ModelFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), ModelFolderName);

    public static List<AIModel> ListModels { get; set; } = [
        new(){ Name="phi-3", Description = "Phi-3 is a family of lightweight 3B (Mini) and 14B (Medium) state-of-the-art open models by Microsoft.", DownloadUrl = "https://is3.cloudhost.id/cloudfile/models/phi3.7z" , Creator = "Microsoft", FolderName="phi3", IsVision=true },
        new(){ Name="gemma", Description = "Gemma is a family of lightweight, state-of-the-art open models built by Google DeepMind. Updated to version 1.1", DownloadUrl = "https://is3.cloudhost.id/cloudfile/models/gemma7b.7z" , Creator = "Google Deepmind", FolderName="gemma" },
        new(){ Name="mistral", Description = "The 7B model released by Mistral AI, updated to version 0.3.\r\nTools\r\n7B", DownloadUrl = "https://is3.cloudhost.id/cloudfile/models/mistral7b.7z" , Creator = "Mistral AI", FolderName="mistral" },
        new(){ Name="llama3", Description = "Meta Llama 3: The most capable openly available LLM to date", DownloadUrl = "" , Creator = "Meta AI", FolderName="llama" },
        ];
}