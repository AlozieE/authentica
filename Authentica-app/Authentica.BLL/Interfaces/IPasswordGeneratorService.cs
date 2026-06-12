namespace Authentica.BLL.Interfaces
{
    public interface IPasswordGeneratorService
    {
        string Generate(int length);
    }
}
