namespace Roanoke
{
    public interface IFolderSizeCalculator
    {
        FolderSizeResult Calculate(string path);
    }
}