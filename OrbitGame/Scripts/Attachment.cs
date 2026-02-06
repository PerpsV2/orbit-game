namespace OrbitGame;

public record struct Attachment(
    KinematicObject Parent,
    SD_Vector2 RelativePosition
    );