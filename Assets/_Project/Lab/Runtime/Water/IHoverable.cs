using UnityEngine;

public interface IHoverable
{
    void SetHoverState(bool active);
}

public interface IInteractionPromptProvider
{
    bool IsInteractionActive { get; }
    InteractionPromptPrefabView InteractionPromptPrefab { get; }
    Transform InteractionPromptAnchor { get; }
    Vector2 InteractionPromptScreenOffset { get; }
}
