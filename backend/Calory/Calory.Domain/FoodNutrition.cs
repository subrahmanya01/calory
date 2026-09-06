namespace Calory.Domain
{
    public sealed class FoodNutrition
    {
        public Guid Id { get; set; }
        public Guid FoodEntryId { get; set; }
        public decimal Calories { get; set; }
        public decimal ProteinG { get; set; }
        public decimal CarbohydratesG { get; set; }
        public decimal FatG { get; set; }
        public decimal FiberG { get; set; }
        public decimal SugarG { get; set; }
        public decimal SodiumMg { get; set; }
        public decimal CalciumMg { get; set; }
        public decimal IronMg { get; set; }
        public decimal MagnesiumMg { get; set; }
        public decimal PotassiumMg { get; set; }
        public decimal ZincMg { get; set; }
        public decimal VitaminAMcg { get; set; }
        public decimal VitaminB1Mg { get; set; }
        public decimal VitaminB2Mg { get; set; }
        public decimal VitaminB3Mg { get; set; }
        public decimal VitaminB6Mg { get; set; }
        public decimal VitaminB12Mcg { get; set; }
        public decimal VitaminCMg { get; set; }
        public decimal VitaminDMcg { get; set; }
        public decimal VitaminEMg { get; set; }
        public decimal VitaminKMcg { get; set; }
    }
}
