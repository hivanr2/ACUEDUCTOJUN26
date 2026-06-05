namespace ACUEDUCTO.API.Options;

public sealed class OcrOptions
{
    public const string SectionName = "Ocr";

    /// <summary>
    /// Proporción (0..1) del alto de la imagen a recortar desde abajo para priorizar la ventana blanca.
    /// </summary>
    public double BottomCropHeightRatio { get; set; } = 0.55;

    /// <summary>
    /// Umbral mínimo de confianza (0..100) para decidir si se intenta preprocesamiento.
    /// </summary>
    public double MinConfidenceToSkipPreprocess { get; set; } = 60;

    /// <summary>
    /// Escala aplicada al preprocesado (resize) antes de OCR.
    /// </summary>
    public int PreprocessScale { get; set; } = 2;

    /// <summary>
    /// Si la lectura detectada tiene este largo o más, se asume que incluye "litros" al final.
    /// </summary>
    public int LitersSuffixMinTotalDigits { get; set; } = 7;

    /// <summary>
    /// Máximo de dígitos de m3 que se considera válido (parte izquierda).
    /// </summary>
    public int MaxM3Digits { get; set; } = 6;

    /// <summary>
    /// Mínimo de dígitos de m3 que se considera válido.
    /// </summary>
    public int MinM3Digits { get; set; } = 4;

    /// <summary>
    /// Idioma utilizado en el OCR.
    /// </summary>
    public string Language { get; set; } = "spa";

    /// <summary>
    /// Ruta al directorio tessdata. Si está vacío, se usa {ContentRoot}/tessdata.
    /// </summary>
    public string? TessdataPath { get; set; }

    public bool NumericOnly { get; set; } = true;

    public float Contrast { get; set; } = 1.1f;

    public float SharpenSigma { get; set; } = 1.0f;

    /// <summary>
    /// Alto mínimo en pixeles que debe tener el recorte inferior. Si no se cumple, se evita recortar.
    /// </summary>
    public int MinBottomCropHeightPx { get; set; } = 80;

    /// <summary>
    /// Ancho mínimo en pixeles que debe tener la imagen para intentar OCR.
    /// </summary>
    public int MinImageWidthPx { get; set; } = 60;

    /// <summary>
    /// Alto mínimo en pixeles que debe tener la imagen para intentar OCR.
    /// </summary>
    public int MinImageHeightPx { get; set; } = 30;

    public bool UseGptVisionAlways { get; set; } = true;

    public string? GptVisionEndpoint { get; set; }

    public string? GptVisionApiKey { get; set; }

    public string GptVisionModel { get; set; } = "gpt-4o-mini";

    /// <summary>
    /// "responses" (OpenAI Responses API) or "chat" (Chat Completions compatible).
    /// </summary>
    public string GptVisionApiStyle { get; set; } = "chat";
}
