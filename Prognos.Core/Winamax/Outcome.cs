using System;

namespace Prognos.Core.Winamax
{
    public struct Outcome
    {
        public string Id { get; }
        public string Code { get; }
        public string Label { get; }
        public float Odd { get; }

        public Outcome(string id, string code, string label, float odd)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Code = code ?? throw new ArgumentNullException(nameof(code));
            Label = label ?? throw new ArgumentNullException(nameof(label));
            Odd = odd;
        }
    }
}
