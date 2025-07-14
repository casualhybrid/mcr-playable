using UnityEngine;

/// <summary>
/// Profile represnting basic information about a facebook user
/// </summary>
public class FaceBookUserProfile
{
    public readonly string avatarURL;
    public readonly string userName;
    public readonly string userID;
    public Texture2D userAvatarTexture { get; set; }

    public Sprite userAvatarSprite
    {
        get
        {
            if (spriteAvatar == null && userAvatarTexture != null)
            {
                spriteAvatar = Sprite.Create(userAvatarTexture, new Rect(Vector3.zero, new Vector2(userAvatarTexture.width, userAvatarTexture.height)), Vector2.zero);
            }

            return spriteAvatar;
        }
    }

    private Sprite spriteAvatar;

    public FaceBookUserProfile(string _avatarURL, string _userName, string _userID)
    {
        avatarURL = _avatarURL;
        userName = _userName;
        userID = _userID;
    }
}