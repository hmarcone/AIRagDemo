using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI.Ollama;

//Antes de rodar, certifique-se de ter o Ollama instalado e rodando localmente, e que os modelos "deepseek-r1:8b" e "nomic-embed-text" estejam disponíveis.
//E que o arquivo "politica.txt" esteja presente no diretório do projeto \bin\Debug\net10.0, contendo o manual de política da empresa que você deseja consultar.

Console.WriteLine("\nConsultando um Manual de Política da Empresa (RAG)\n");

var config = new OllamaConfig
{
	Endpoint = "http://localhost:11434",
	TextModel = new OllamaModelConfig("deepseek-r1:8b", 131072),
	EmbeddingModel = new OllamaModelConfig("nomic-embed-text", 768)
};


var memoryBuilder = new KernelMemoryBuilder()
	.WithOllamaTextGeneration(config)
	.WithOllamaTextEmbeddingGeneration(config)
	.WithCustomTextPartitioningOptions(new Microsoft.KernelMemory.Configuration.TextPartitioningOptions
	{
		MaxTokensPerParagraph = 256,
		OverlappingTokens = 30
	});

var memory = memoryBuilder.Build();


Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Iniciando Ingestão de Documentos ...");

try
{
	await memory.ImportDocumentAsync(
		filePath: "politica.txt",
		documentId: "POL001");

	Console.ForegroundColor = ConsoleColor.White;
	Console.WriteLine("Documento 'politica.txt' ingerido com sucesso!!");
	Console.ResetColor();

}
catch (Exception ex)
{
	Console.ForegroundColor = ConsoleColor.Red;
	Console.WriteLine("ERRO durante a ingestão. Verifique se o modelo está correto!");
	Console.WriteLine(ex.Message);
	Console.ResetColor();
	return;	
}

Console.WriteLine("Modelo pronto para perguntas.\n");
Console.ResetColor();


while (true){
	Console.Write("Pergunta (Digite 'sair' para encerrar): ");
	var pergunta = Console.ReadLine();

	if(pergunta?.ToLower() == "sair")
		break;

	if(string.IsNullOrWhiteSpace(pergunta))
		continue;

	var prompt = $"Responda somente com base nos documentos fornecidos. \n\n Pergunta: {pergunta}";

	var resposta = await memory.AskAsync(prompt);

	Console.ForegroundColor = ConsoleColor.Yellow;
	Console.WriteLine($"\nAssistente: {resposta.Result}");
	Console.ResetColor();


	Console.BackgroundColor = ConsoleColor.Cyan;
	Console.ForegroundColor = ConsoleColor.Black;

	Console.WriteLine("\n--- Fontes Encontradas ---");

	if(resposta.RelevantSources.Count == 0){
		Console.WriteLine("Nenhuma fonte relevante encontrada.");
	}else{
		foreach(var source in resposta.RelevantSources){
			Console.WriteLine($"- Arquivo: {source.SourceName}, Trecho: {source.Partitions.FirstOrDefault()?.Text}");
		}
	}

	Console.ResetColor();
	Console.WriteLine("-------------------------\n");

}
