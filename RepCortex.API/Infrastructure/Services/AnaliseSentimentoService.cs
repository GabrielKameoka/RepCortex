using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using RepCortex.Domain.Interfaces.Service;

namespace RepCortex.Infrastructure.Services;

public class AvaliacaoData
{
    public string Texto { get; set; } = string.Empty;
    public string Sentimento { get; set; } = string.Empty;
}

public class AvaliacaoPrediction
{
    [ColumnName("PredictedLabel")]
    public string SentimentoPrevisto { get; set; } = string.Empty;
}

/// <summary>
/// Serviço de análise de sentimento utilizando uma implementação customizada e otimizada do algoritmo VADER adaptado para o Português.
/// </summary>
public class AnaliseSentimentoService : IAnaliseSentimentoService
{
    private static readonly Dictionary<string, double> Lexicon = new(StringComparer.OrdinalIgnoreCase)
    {
        // Positivos (Valores de valência adaptados de +1.0 a +4.0)
        { "excelente", 3.1 },
        { "excelentes", 3.1 },
        { "otimo", 3.1 },
        { "ótimo", 3.1 },
        { "otimos", 3.1 },
        { "ótimos", 3.1 },
        { "otima", 3.1 },
        { "ótima", 3.1 },
        { "otimas", 3.1 },
        { "ótimas", 3.1 },
        { "bom", 1.9 },
        { "bons", 1.9 },
        { "boa", 1.9 },
        { "boas", 1.9 },
        { "maravilhoso", 3.0 },
        { "maravilhosa", 3.0 },
        { "maravilhosos", 3.0 },
        { "maravilhosas", 3.0 },
        { "amei", 3.2 },
        { "amo", 3.2 },
        { "amar", 3.2 },
        { "amamos", 3.2 },
        { "adorou", 2.5 },
        { "adorei", 2.5 },
        { "adoro", 2.5 },
        { "adoramos", 2.5 },
        { "gostei", 2.0 },
        { "gosto", 2.0 },
        { "gostamos", 2.0 },
        { "recomendo", 2.5 },
        { "recomendado", 2.5 },
        { "recomendadissimo", 3.2 },
        { "recomendadíssimo", 3.2 },
        { "perfeito", 3.0 },
        { "perfeita", 3.0 },
        { "perfeitos", 3.0 },
        { "perfeitas", 3.0 },
        { "sensacional", 3.0 },
        { "sensacionais", 3.0 },
        { "legal", 1.8 },
        { "legais", 1.8 },
        { "rapido", 1.5 },
        { "rápido", 1.5 },
        { "rapidos", 1.5 },
        { "rápidos", 1.5 },
        { "rapidez", 1.8 },
        { "util", 1.7 },
        { "útil", 1.7 },
        { "uteis", 1.7 },
        { "úteis", 1.7 },
        { "lindo", 2.0 },
        { "linda", 2.0 },
        { "lindos", 2.0 },
        { "lindas", 2.0 },
        { "surpreendeu", 2.2 },
        { "surpreendente", 2.5 },
        { "facil", 1.5 },
        { "fácil", 1.5 },
        { "faceis", 1.5 },
        { "fáceis", 1.5 },
        { "sucesso", 2.5 },
        { "feliz", 2.2 },
        { "felizes", 2.2 },
        { "satisfeito", 2.0 },
        { "satisfeita", 2.0 },
        { "satisfeitos", 2.0 },
        { "satisfeitas", 2.0 },
        { "satisfacao", 2.2 },
        { "satisfação", 2.2 },
        { "agradavel", 1.9 },
        { "agradável", 1.9 },
        { "divertido", 1.9 },
        { "incrivel", 3.0 },
        { "incrível", 3.0 },
        { "maravilha", 2.5 },
        { "sensacionalmente", 3.0 },

        // Negativos (Valores de valência adaptados de -1.0 a -4.0)
        { "ruim", -1.9 },
        { "ruins", -1.9 },
        { "pessimo", -3.1 },
        { "péssimo", -3.1 },
        { "pessimos", -3.1 },
        { "péssimos", -3.1 },
        { "pessima", -3.1 },
        { "péssima", -3.1 },
        { "pessimas", -3.1 },
        { "péssimas", -3.1 },
        { "horrivel", -3.0 },
        { "horrível", -3.0 },
        { "horriveis", -3.0 },
        { "horríveis", -3.0 },
        { "odiei", -3.2 },
        { "odeio", -3.2 },
        { "odiar", -3.2 },
        { "odiamos", -3.2 },
        { "quebrou", -2.0 },
        { "quebrado", -2.0 },
        { "quebrada", -2.0 },
        { "defeito", -2.5 },
        { "defeituoso", -2.5 },
        { "defeituosa", -2.5 },
        { "danificado", -2.5 },
        { "danificada", -2.5 },
        { "problema", -1.5 },
        { "problemas", -1.5 },
        { "estragou", -2.0 },
        { "estragado", -2.0 },
        { "estragada", -2.0 },
        { "lento", -1.5 },
        { "lenta", -1.5 },
        { "lentidao", -1.8 },
        { "lentidão", -1.8 },
        { "demorou", -1.5 },
        { "demorada", -1.5 },
        { "demorado", -1.5 },
        { "atrasou", -1.5 },
        { "atrasado", -1.5 },
        { "atrasada", -1.5 },
        { "decepcionado", -2.5 },
        { "decepcionada", -2.5 },
        { "decepcionados", -2.5 },
        { "decepcionadas", -2.5 },
        { "decepcao", -2.5 },
        { "decepção", -2.5 },
        { "pior", -2.5 },
        { "piores", -2.5 },
        { "dificil", -1.5 },
        { "difícil", -1.5 },
        { "dificeis", -1.5 },
        { "difíceis", -1.5 },
        { "caro", -1.2 },
        { "cara", -1.2 },
        { "triste", -2.0 },
        { "tristes", -2.0 },
        { "insatisfeito", -2.0 },
        { "insatisfeita", -2.0 },
        { "insatisfeitos", -2.0 },
        { "insatisfeitas", -2.0 },
        { "falhou", -2.0 },
        { "falha", -1.5 },
        { "falhas", -1.5 },
        { "erro", -1.5 },
        { "erros", -1.5 },
        { "sujo", -1.5 },
        { "suja", -1.5 },
        { "pobre", -1.0 },
        { "lixo", -2.8 },
        { "porcaria", -2.8 },
        { "merda", -3.2 },
        { "bosta", -3.2 },
        { "mal", -1.5 },
        { "pessimamente", -3.0 }
    };

    private static readonly HashSet<string> Negations = new(StringComparer.OrdinalIgnoreCase)
    {
        "não", "nao", "nem", "nunca", "jamais", "nada", "sem", "ninguém", "ninguem", "tampouco"
    };

    private static readonly Dictionary<string, double> Boosters = new(StringComparer.OrdinalIgnoreCase)
    {
        { "muito", 0.291 },
        { "super", 0.291 },
        { "extremamente", 0.35 },
        { "bastante", 0.25 },
        { "demais", 0.25 },
        { "mais", 0.15 },
        { "tão", 0.20 },
        { "tao", 0.20 },
        { "completamente", 0.30 },
        { "totalmente", 0.30 },
        { "altamente", 0.25 }
    };

    private static readonly Dictionary<string, double> Dampeners = new(StringComparer.OrdinalIgnoreCase)
    {
        { "pouco", -0.291 },
        { "quase", -0.15 },
        { "apenas", -0.10 },
        { "ligeiramente", -0.15 },
        { "meio", -0.10 }
    };

    private static readonly HashSet<string> ButConjunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        "mas", "porem", "porém", "entretanto", "todavia", "contudo"
    };

    public Task<string> AnalisarSentimentoAsync(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return Task.FromResult("Neutro");

        // 1. Tokenização e pré-processamento (retorna os tokens e se eles terminam com barreira de cláusula)
        var (tokens, hasBoundary) = Tokenize(texto);
        if (tokens.Count == 0)
            return Task.FromResult("Neutro");

        // 2. Verifica se todo o texto está em letras maiúsculas
        bool isEntireTextAllCaps = texto.Equals(texto.ToUpper(), StringComparison.Ordinal) && 
                                   !texto.Equals(texto.ToLower(), StringComparison.Ordinal);

        // 3. Verifica pontuações de intensidade (exclamações e interrogações)
        int exclamationCount = texto.Count(c => c == '!');
        int questionCount = texto.Count(c => c == '?');
        double punctBoost = Math.Min(exclamationCount, 4) * 0.292 + Math.Min(questionCount, 3) * 0.18;

        // 4. Encontra qualquer conjunção contrastiva (ex: "mas")
        int? conjIdx = null;
        for (int i = 0; i < tokens.Count; i++)
        {
            if (ButConjunctions.Contains(tokens[i]))
            {
                conjIdx = i;
                break;
            }
        }

        double totalScore = 0;
        int sentimentWordsCount = 0;

        for (int i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];

            if (Lexicon.TryGetValue(token, out double baseValence))
            {
                sentimentWordsCount++;
                double valence = baseValence;

                // Regra ALL CAPS: Se apenas a palavra atual estiver em maiúscula, amplifica a valência
                if (!isEntireTextAllCaps && IsAllCaps(token))
                {
                    valence += 0.733 * Math.Sign(baseValence);
                }

                // Regra de Intensificadores (Boosters e Dampeners) nos 3 tokens anteriores
                double modifier = 0;
                for (int dist = 1; dist <= 3; dist++)
                {
                    int prevIdx = i - dist;
                    if (prevIdx >= 0)
                    {
                        var prevToken = tokens[prevIdx];
                        if (Boosters.TryGetValue(prevToken, out double bVal))
                        {
                            modifier += bVal * (1.0 - (dist - 1) * 0.05);
                        }
                        else if (Dampeners.TryGetValue(prevToken, out double dVal))
                        {
                            modifier += dVal * (1.0 - (dist - 1) * 0.05);
                        }

                        // Se o token percorrido possui barreira de pontuação logo após ele, 
                        // interrompe a propagação para os anteriores.
                        if (hasBoundary[prevIdx])
                        {
                            break;
                        }
                    }
                }
                valence += modifier * Math.Sign(baseValence);

                // Regra de Negação nos 3 tokens anteriores
                bool isNegated = false;
                for (int dist = 1; dist <= 3; dist++)
                {
                    int prevIdx = i - dist;
                    if (prevIdx >= 0)
                    {
                        if (Negations.Contains(tokens[prevIdx]))
                        {
                            isNegated = true;
                            break;
                        }

                        // Se o token percorrido possui barreira de pontuação logo após ele, 
                        // interrompe a propagação de negações de antes dele.
                        if (hasBoundary[prevIdx])
                        {
                            break;
                        }
                    }
                }
                if (isNegated)
                {
                    valence *= -0.74;
                }

                // Regra da Conjunção Contrastiva ("mas"): reduz peso antes do "mas", aumenta depois
                if (conjIdx.HasValue)
                {
                    if (i < conjIdx.Value)
                    {
                        valence *= 0.5;
                    }
                    else if (i > conjIdx.Value)
                    {
                        valence *= 1.5;
                    }
                }

                totalScore += valence;
            }
        }

        if (sentimentWordsCount == 0)
        {
            return Task.FromResult("Neutro");
        }

        // Aplica o booster de pontuação ao score total
        if (totalScore > 0)
        {
            totalScore += punctBoost;
        }
        else if (totalScore < 0)
        {
            totalScore -= punctBoost;
        }

        // Normalização padrão do VADER para escala de -1.0 a +1.0
        double compound = totalScore / Math.Sqrt(totalScore * totalScore + 15.0);

        if (compound >= 0.05)
            return Task.FromResult("Positivo");
        if (compound <= -0.05)
            return Task.FromResult("Negativo");

        return Task.FromResult("Neutro");
    }

    private static (List<string> Tokens, List<bool> HasBoundary) Tokenize(string text)
    {
        var tokens = new List<string>();
        var hasBoundary = new List<bool>();
        var rawTokens = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var rt in rawTokens)
        {
            var token = rt;
            bool endedWithBoundary = rt.Length > 0 && (rt[^1] == ',' || rt[^1] == ';' || rt[^1] == '.' || rt[^1] == '!' || rt[^1] == '?');
            
            while (token.Length > 0 && char.IsPunctuation(token[0]) && token[0] != '!')
            {
                token = token[1..];
            }
            while (token.Length > 0 && char.IsPunctuation(token[^1]) && token[^1] != '!')
            {
                token = token[..^1];
            }
            if (token.Length > 0)
            {
                tokens.Add(token);
                hasBoundary.Add(endedWithBoundary);
            }
        }
        return (tokens, hasBoundary);
    }

    private static bool IsAllCaps(string word)
    {
        return word.Length > 1 && word.All(c => !char.IsLetter(c) || char.IsUpper(c));
    }
}