public enum BlackListLevel
{
    Low, Medium
}

[System.Serializable]
public struct BlackListBehaviour
{
    public BlackListLevel BlackListLevel;
}