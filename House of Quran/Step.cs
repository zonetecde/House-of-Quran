namespace House_of_Quran
{
    internal class Step
    {
        public Step(StepType type, int[] posInWrapPanel)
        {
            Type = type;
            PosInWrapPanel = posInWrapPanel;
        }

        internal StepType Type { get; set; }
        internal int[] PosInWrapPanel { get; set; }
    }

    internal enum StepType
    {
        LECTURE_MOT,
        LECTURE_VERSET,
        REPETITION,
        VOID,
    }
}