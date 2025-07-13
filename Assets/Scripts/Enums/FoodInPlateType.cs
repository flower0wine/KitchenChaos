/// <summary>
/// 定义所有可能的食材类型
/// </summary>
public enum FoodInPlateType
{
    None = FoodType.None,
    Bread = FoodType.Bread,
    CheeseSlice = FoodType.CheeseSlice,
    TomatoSlice = FoodType.TomatoSlice,
    CabbageSlice = FoodType.CabbageSlice,
    CookedMeat = FoodType.CookedMeat,
    BurnedMeat = FoodType.BurnedMeat,
} 

public static class FoodInPlateTypeExtensions
{
    public static FoodType ToFoodType(this FoodInPlateType foodInPlateType)
    {
        return (FoodType)foodInPlateType;
    }
}