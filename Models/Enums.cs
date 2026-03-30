namespace Top5.Models
{
    public enum ShiftType
    {
        Matin,
        ApresMidi,
        Nuit
    }

    public enum ControlState
    {
        NonRenseigne,
        B,   // Conforme (Vert)
        AA,  // A améliorer (Orange)
        NC   // Non conforme (Rouge)
    }
}