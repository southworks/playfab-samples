using DataSeeder.Configuration;

namespace DataSeeder.Configurer
{
    public interface IDataSeederConfigurer
    {
        DataSeederConfig Configure();
    }
}
