namespace Web.Data.Entities
{
    public class FavoriteRecipe
    {
        public int Id { get; set; }
        public User User { get; set; }
        public Recipe Recipe { get; set; }
    }
}
