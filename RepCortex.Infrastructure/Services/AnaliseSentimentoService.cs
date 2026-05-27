using Microsoft.ML;
using Microsoft.ML.Data;
using RepCortex.Domain.Interfaces.Service;

namespace RepCortex.Infrastructure.Services;

// 1. Classe que representa a ENTRADA dos dados (O que a IA vai ler)
public class AvaliacaoData
{
    public string Texto { get; set; } = string.Empty;
    public string Sentimento { get; set; } = string.Empty;
}

// 2. Classe que representa a SAÍDA da IA (O que ela vai responder)
public class AvaliacaoPrediction
{
    [ColumnName("PredictedLabel")]
    public string SentimentoPrevisto { get; set; } = string.Empty;
}

public class AnaliseSentimentoService : IAnaliseSentimentoService
{
    private readonly MLContext _mlContext;
    private ITransformer _modelo;

    public AnaliseSentimentoService()
    {
        _mlContext = new MLContext();
        
        // Treina o modelo exatamente no momento em que a classe é instanciada (quando a API sobe)
        _modelo = TreinarModelo(); 
    }

    private ITransformer TreinarModelo()
    {
        // 1. O DATASET (Exemplos reais para a IA aprender os padrões de linguagem)
        // Em um sistema real, isso viria de um banco de dados com 10.000 linhas ou de um arquivo .csv
        var dadosTreinamento = new List<AvaliacaoData>
        {
            new() { Texto = "O produto é excelente, amei demais!", Sentimento = "Positivo" },
            new() { Texto = "Chegou super rápido, recomendo.", Sentimento = "Positivo" },
            new() { Texto = "Muito bom, material de qualidade.", Sentimento = "Positivo" },
            new() { Texto = "Horrível, quebrou no primeiro dia.", Sentimento = "Negativo" },
            new() { Texto = "Odiei, a caixa veio rasgada e faltou peça.", Sentimento = "Negativo" },
            new() { Texto = "Péssimo atendimento e produto ruim.", Sentimento = "Negativo" },
            new() { Texto = "É ok, pelo preço vale a pena.", Sentimento = "Neutro" },
            new() { Texto = "Normal, nada demais.", Sentimento = "Neutro" },
            new() { Texto = "Veio como descrito, mas não me surpreendeu.", Sentimento = "Neutro" }
        };

        // Carrega os dados para o formato que o ML.NET entende
        var dadosView = _mlContext.Data.LoadFromEnumerable(dadosTreinamento);

        // 2. O PIPELINE (A linha de montagem da Inteligência)
        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(inputColumnName: "Sentimento", outputColumnName: "Label")
            // Converte o texto em números (Features) para a matemática funcionar
            .Append(_mlContext.Transforms.Text.FeaturizeText(inputColumnName: "Texto", outputColumnName: "Features"))
            // Usa o algoritmo de classificação multiclasse (SdcaMaximumEntropy)
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
            // Converte o número final de volta para texto (Positivo/Negativo/Neutro)
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

        // 3. A MÁGICA (A IA estuda os dados e gera o "Cérebro")
        return pipeline.Fit(dadosView);
    }

    public Task<string> AnalisarSentimentoAsync(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return Task.FromResult("Neutro");

        // Cria o motor de predição com o cérebro que acabamos de treinar
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<AvaliacaoData, AvaliacaoPrediction>(_modelo);

        // Pede para a IA prever o sentimento de uma frase que ela NUNCA viu antes
        var predicao = predictionEngine.Predict(new AvaliacaoData { Texto = texto });

        return Task.FromResult(predicao.SentimentoPrevisto);
    }
}