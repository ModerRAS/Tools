
using System;
using Microsoft.AspNetCore.Components;

namespace Tools.Pages
{
    public partial class CompoundInterest : ComponentBase
    {
        private double Principal { get; set; } = 5000;
        private double InterestRate { get; set; } = 5;
        private int Years { get; set; } = 10;
        private double total { get; set; } = 0;
        private string Total { get; set; }

        private void Calculate()
        {
            var total = Principal * Math.Pow(1 + InterestRate / (1200.0), Years * 12);
            Total = total.ToString("C");
        }
    }
}