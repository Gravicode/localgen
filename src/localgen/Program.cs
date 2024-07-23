using localgen.Data;
using localgen.Helpers;
using localgen.Models;
using Microsoft.ML.OnnxRuntimeGenAI;
using SevenZipExtractor;
using Spectre.Console;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using Color = System.Drawing.Color;
using Console = Colorful.Console;
namespace localgen;
class Program
{
    static bool runForever = true;
    static AppState state = new();
    static void Main(string[] args)
    {

        RunCli();
        while (runForever)
        {
            Thread.Sleep(100);
        }
    }

    static async void RunCli()
    {
        Welcome();
        await InitializeClient();


        while (runForever)
        {
            string cmd = InputString("Command [? for help]:", null, false);
            switch (cmd)
            {
                case "?":
                    Menu();
                    break;
                case "q":
                    runForever = false;

                    break;
                case "c":
                case "cls":
                case "clear":
                    Console.Clear();
                    break;

                case "chat":
                    await Chat();
                    break;
                case "download":
                    await ListModel();
                    break; 
                case "api":
                    await RunApi();
                    break;
            }
        }
    }

    static void Welcome()
    {
        int DA = 244;
        int V = 212;
        int ID = 255;
        Console.WriteAscii("Welcome to", Color.FromArgb(DA, V, ID));

        DA -= 18;
        V -= 36;

        Console.WriteAscii("Local-Gen", Color.FromArgb(DA, V, ID));

        DA -= 18;
        V -= 36;

        Console.WriteAscii("Local AI Model", Color.FromArgb(DA, V, ID));

    }
    static async Task InitializeClient()
    {
        if (!Directory.Exists(AppConstants.ModelFolder))
        {
            Directory.CreateDirectory(AppConstants.ModelFolder);
        }

    }
    static string InputString(string question, string defaultAnswer, bool allowNull)
    {
        while (true)
        {
            Console.Write(question);

            if (!String.IsNullOrEmpty(defaultAnswer))
            {
                Console.Write(" [" + defaultAnswer + "]");
            }

            Console.Write(" ");

            string userInput = Console.ReadLine();

            if (String.IsNullOrEmpty(userInput))
            {
                if (!String.IsNullOrEmpty(defaultAnswer)) return defaultAnswer;
                if (allowNull) return null;
                else continue;
            }

            return userInput;
        }
    }
    static bool InputBoolean(string question, bool yesDefault)
    {
        Console.Write(question);

        if (yesDefault) Console.Write(" [Y/n]? ");
        else Console.Write(" [y/N]? ");

        string userInput = Console.ReadLine();

        if (String.IsNullOrEmpty(userInput))
        {
            if (yesDefault) return true;
            return false;
        }

        userInput = userInput.ToLower();

        if (yesDefault)
        {
            if (
                (String.Compare(userInput, "n") == 0)
                || (String.Compare(userInput, "no") == 0)
               )
            {
                return false;
            }

            return true;
        }
        else
        {
            if (
                (String.Compare(userInput, "y") == 0)
                || (String.Compare(userInput, "yes") == 0)
               )
            {
                return true;
            }

            return false;
        }
    }
    static async Task ListModel()
    {
        try
        {
            var selectedModel = await SelectModel();
            //downloadModel
            await DownloadModel(selectedModel);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]Download model error[/]");
            AnsiConsole.WriteException(ex);
        }
    }

    static async Task<AIModel> SelectModel()
    {
        // Create a table
        var table = new Table();

        // Add some columns
        table.AddColumn(new TableColumn("No").LeftAligned());
        table.AddColumn(new TableColumn("Model").LeftAligned());
        table.AddColumn(new TableColumn("Desc").LeftAligned());
        table.AddColumn(new TableColumn("Creator").LeftAligned());
        var count = 1;
        foreach (var model in AppConstants.ListModels)
        {
            var storageStr = string.Empty;
            table.AddRow(count.ToString(), model.Name, model.Description, model.Creator);
            count++;
        }
        // Render the table to the console
        AnsiConsole.Write(table);

        var modelName = AnsiConsole.Prompt(
      new SelectionPrompt<string>()
     .Title("Please select model to chat ?")
     .AddChoices(AppConstants.ListModels.Select(x => x.Name).ToArray()));
        Console.WriteLine("");
        var selectedModel = AppConstants.ListModels.FirstOrDefault(x => x.Name == modelName);
        return selectedModel;
    }

    static async Task RunApi()
    {
        try
        {
            var selectedModel = await SelectModel();
            //chat
            using (var api = new OpenAIAPI(state, selectedModel))
            {
                api.RunApi();
                AnsiConsole.MarkupLine("[green]Press any key to stop API[/]");
                var xx = Console.ReadKey();
                api.StopApi();
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteLine($"Error: {ex.ToString()}");

        }
    }
    static async Task Chat()
    {
        try
        {
            var selectedModel = await SelectModel();
            //chat
            using (var chatSession = new ChatWithModel(selectedModel))
            {
                chatSession.RunChat();
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteLine($"Error: {ex.ToString()}");

        }
    }
    async static Task DownloadModel(AIModel selectedModel)
    {
        try
        {
            var downloadFileUrl = selectedModel.DownloadUrl;
            var destinationFolder = Path.Combine(AppConstants.ModelFolder, selectedModel.FolderName);
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }
            var destinationFilePath = Path.Combine(destinationFolder, Path.GetFileName(downloadFileUrl));
            if (File.Exists(destinationFilePath))
            {
                var execute = InputBoolean("Model is already exist, do you want to download and overwrite ?", true);
                if (!execute)
                {
                    AnsiConsole.Write("Download model cancelled.");
                    return;
                }
            }
            double? progress = 0d;
            AnsiConsole.Progress()
                .Start(ctx =>
                {
                    // Define tasks
                    var task1 = ctx.AddTask("[green]Download Model[/]");
                    using (var client = new HttpClientDownloadWithProgress(downloadFileUrl, destinationFilePath))
                    {
                        client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                        {
                            progress = progressPercentage;
                            if (progress.HasValue)
                                task1.Value(progress.Value);
                            //Console.WriteLine($"{progressPercentage}% ({totalBytesDownloaded}/{totalFileSize})");
                        };

                        client.StartDownload().GetAwaiter().GetResult();
                        
                    }
                    while (!ctx.IsFinished)
                    {
                        Thread.Sleep(1000);
                    }


                });
            AnsiConsole.MarkupLine("[green]Model has been downloaded.[/]");
            Thread.Sleep(500);
            AnsiConsole.MarkupLine("[yellow]Extracting file..please wait[/]");
            //extract file..
            using (ArchiveFile archiveFile = new ArchiveFile(destinationFilePath))
            {
                archiveFile.Extract(destinationFolder, true); // extract all
                                                              //move file above if there is sub folder in model dir
                var destDir = new DirectoryInfo(destinationFolder);
                foreach (var folder in destDir.GetDirectories())
                {
                    var ParentFolder = Path.Combine(folder.FullName, "..");
                    try
                    {
                        foreach (var file in folder.GetFiles())
                        {
                            file.MoveTo($@"{ParentFolder}\{file.Name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine("[red]move file is failed[/]");
                        AnsiConsole.WriteException(ex);
                    }
                    finally
                    {
                        folder.Delete();
                    }
                   
                   
                }
            }
            AnsiConsole.MarkupLine("[blue]Model extraction completed.[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]Fail to download model:[/]");
            AnsiConsole.WriteException(ex);
        }

    }
    static void Menu()
    {
        Console.WriteLine("");
        AnsiConsole.WriteLine("Available commands:");

        // Create a table
        var table = new Table();

        // Add some columns
        table.AddColumn("No");
        table.AddColumn(new TableColumn("Category").LeftAligned());
        table.AddColumn(new TableColumn("Command").LeftAligned());
        table.AddColumn(new TableColumn("Description").LeftAligned());

        // Add some rows
        table.AddRow("1", "global", "[green]?[/]", "Help, this Menu");
        table.AddRow("2", "global", "[green]cls[/]", "Clear the screen");
        table.AddRow("3", "global", "[green]q[/]", "Quit");
        table.AddRow("4", "Chat", "[green]chat[/]", "Chat with Model");
        table.AddRow("5", "Download Model", "[green]download[/]", "Download generative model");
        table.AddRow("6", "Model as an API", "[green]api[/]", "Run model as Open AI compatible API");

        // Render the table to the console
        AnsiConsole.Write(table);

    }

}
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

public class OpenAIAPI:IDisposable
{
    public WebApplication app { get; set; }
    public bool IsConfigured { get; set; } = false;
    public bool IsRunning { get; set; } = false;
    public string LocalUrl { get; set; } = "http://localhost";
    public int Port { get; set; } = 8080;
    public AIModel Model { get; set; }
    public AppState state { get; set; }
    public ChatWithModel chatEngine { get; set; }
    public OpenAIAPI(AppState state, AIModel model)
    {
        this.state = state;
        this.Model = model;
        Setup();
    }
    public OpenAIAPI(AppState state, AIModel model, int port)
    {
        this.state = state;
        this.Model = model;
        this.Port = port;
        Setup();
    }

    void Setup()
    {
        try
        {
            //chat
            chatEngine = new ChatWithModel(this.Model);
            IsConfigured = true;

        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error init model:[/]");
            AnsiConsole.WriteException(ex);
            IsConfigured = false;
        }
    }
    async public void RunApi()
    {
        try
        {
            var ApiUrl = $"{LocalUrl}:{Port}/";
            //services for signalr hub, it's running on different thread
            var args = Environment.GetCommandLineArgs();
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls(ApiUrl);
            //builder.Services.AddSignalR();
            builder.Services.AddSingleton<AppState>(state);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder => builder.AllowAnyOrigin().AllowAnyHeader().WithMethods("GET, PATCH, DELETE, PUT, POST, OPTIONS"));
            });
            app = builder.Build();
            //app.MapHub<ChatHub>("/chathub");

            app.UseCors(x => x
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true) // allow any origin  
            .AllowCredentials());               // allow credentials 
            
            //Open AI compatible completion
            app.MapPost("/v1/chat/completions", async (CompletionRequest req) =>
            {
                var response = new CompletionResponse();
                response.model = this.Model.Name;
                response.id = Guid.NewGuid().ToString();
                if (IsConfigured)
                {
                    var message = await chatEngine.GetCompletion(req.messages.First().content);
                    CompletionChoice choice = new() { message = new() { role = "assistant", content = message } };
                    response.choices = new[] { choice };
                }
                else
                {
                    CompletionChoice choice = new() { message = new() { role = "assistant", content = "Model is not ready, please re-download model." } };
                    response.choices = new[] { choice };
                }
               
                return response;
            });
            AnsiConsole.MarkupLine($"[green]Api is running at {ApiUrl}[/]");
            IsRunning = true;
            await app.RunAsync();


        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]Run Api Error:[/]");
            AnsiConsole.WriteException(ex);
            IsRunning = false;
        }
    }

    public async void StopApi()
    {
        if (IsRunning)
        {
            try
            {
                await app.StopAsync();
                AnsiConsole.MarkupLine("[yellow]Api is stopped[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine("[red]Stop Api failed:[/]");
                AnsiConsole.WriteException(ex);
            }
            finally
            {
                IsRunning = false;
            }

        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]No running Api[/]");
        }
    }

    public void Dispose()
    {
        if (IsConfigured)
        {
            chatEngine.Dispose();
        }
    }
}

public class ChatWithModel : IDisposable
{
    public bool IsConfigured { get; set; }
    public Model model { get; set; }
    public MultiModalProcessor processor { get; set; }
    public TokenizerStream tokenizerStream { get; set; }
    public AIModel selectedModel { get; set; }
    public ChatWithModel(AIModel model)
    {
        this.selectedModel = model;
    }

    void Setup()
    {
        try
        {
            string modelPath = Path.Combine(AppConstants.ModelFolder, selectedModel.FolderName);
            if (!Directory.Exists(modelPath))
            {
                AnsiConsole.WriteLine($"Model is not exist, please download model {selectedModel.Name} first!");
                return;
            }

            model = new Model(modelPath);
            processor = new MultiModalProcessor(model);
            tokenizerStream = processor.CreateStream();
            IsConfigured = true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]Init Model Error:[/]");
            AnsiConsole.WriteException(ex);
            IsConfigured = false;
            throw;
        }
    }
    public void RunChat()
    {
        try
        {
            if (!IsConfigured) Setup();
            var rule = new Rule($"[red]Chat with Model ({selectedModel.Name})[/]");
            AnsiConsole.Write(rule);
            AnsiConsole.MarkupLine("[yellow]type 'quit' to exit chat.[/]");
            while (true)
            {
                string imagePath = string.Empty;
                Images images = null;
                if (selectedModel.IsVision)
                {
                    AnsiConsole.Markup("[green]Image Path (leave empty if no image):[/]");
                    imagePath = Console.ReadLine();
                    if (imagePath == String.Empty)
                    {
                        AnsiConsole.MarkupLine("[red]No image provided[/]");
                    }
                    else
                    {
                        if (!File.Exists(imagePath))
                        {
                            AnsiConsole.WriteException(new Exception("Image file not found: " + imagePath));
                        }
                        else
                            images = Images.Load(imagePath);
                    }
                }

                AnsiConsole.Markup("[green]Prompt:[/]");
                string text = Console.ReadLine();
                if (text.Equals("quit", StringComparison.InvariantCultureIgnoreCase)) break;
                string prompt = "<|user|>\n";
                if (images != null)
                {
                    prompt += "<|image_1|>\n";
                }
                prompt += text + "<|end|>\n<|assistant|>\n";

                AnsiConsole.WriteLine("Processing image and prompt...");
                var inputTensors = processor.ProcessImages(prompt, images);

                AnsiConsole.WriteLine("Generating response...");
                AnsiConsole.Markup("[yellow]Response:[/]");
                using GeneratorParams generatorParams = new GeneratorParams(model);
                generatorParams.SetSearchOption("max_length", 3072);
                generatorParams.SetInputs(inputTensors);

                using var generator = new Generator(model, generatorParams);
                while (!generator.IsDone())
                {
                    generator.ComputeLogits();
                    generator.GenerateNextToken();
                    AnsiConsole.Write(tokenizerStream.Decode(generator.GetSequence(0)[^1]));
                }
                AnsiConsole.WriteLine("");
            }
            AnsiConsole.MarkupLine("[blue]Exit chat..[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]Fail to chat with Model:[/]");
            AnsiConsole.WriteException(ex);
        }
    }
    public async Task<string> GetCompletion(string Input, string imagePath = "")
    {
        try
        {
            if (!IsConfigured) Setup();
            Images images = null;
            if (this.selectedModel.IsVision)
            {

                if (!File.Exists(imagePath))
                {
                    AnsiConsole.WriteException(new Exception("Image file not found: " + imagePath));
                }
                else
                    images = Images.Load(imagePath);
            }
            if (string.IsNullOrEmpty(Input))
            {
                return "Cannot process empty prompt.";
            }
            string prompt = "<|user|>\n";
            if (images != null)
            {
                prompt += "<|image_1|>\n";
            }
            prompt += Input + "<|end|>\n<|assistant|>\n";

            var inputTensors = processor.ProcessImages(prompt, images);

            using GeneratorParams generatorParams = new GeneratorParams(model);
            generatorParams.SetSearchOption("max_length", 3072);
            generatorParams.SetInputs(inputTensors);
            var response = new StringBuilder();
            using var generator = new Generator(model, generatorParams);
            while (!generator.IsDone())
            {
                generator.ComputeLogits();
                generator.GenerateNextToken();
                response.Append(tokenizerStream.Decode(generator.GetSequence(0)[^1]));
            }
            AnsiConsole.WriteLine("response api:" + response);
            return response.ToString();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine("[red]Fail to call Model:[/]");
            AnsiConsole.WriteException(ex);
            return $"Model fail to response: {ex}";
        }
    }
    public void Dispose()
    {
        if (IsConfigured)
        {
            model.Dispose();
            processor.Dispose();
            tokenizerStream.Dispose();
        }
    }
}