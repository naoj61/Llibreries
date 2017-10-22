namespace Controls
{
    public interface IValorControlRestaurable
    {
        bool Modified{ get; }
        void Undo();
    }
}