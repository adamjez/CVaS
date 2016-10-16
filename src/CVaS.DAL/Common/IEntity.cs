namespace CVaS.DAL.Common
{
    public interface IEntity<TKey>
    {
        TKey Id { get; set; }
    }
}
