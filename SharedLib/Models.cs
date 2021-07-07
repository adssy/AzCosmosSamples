namespace AzCosmosDB.Samples.SharedLib
{
    using System;
    using System.Collections.Generic;
    using Bogus;
    using Newtonsoft.Json;


    public interface ICosmosDocument
    {
        public string Id { get; set; }
    }
    public class User : ICosmosDocument
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        
        public List<Order> Orders { get; set; }
    }

    public class Order
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string ItemName { get; set; }
        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }
    }

    public class GamePlayer
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string NickName { get; set; }
        public int Gold { get; set; }
    }
    public class GameItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "pk")]
        public string PlayerId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }

        public GameItem(string player_id, string item_name, int quantity)
        {
            this.Id = Guid.NewGuid().ToString();
            this.PlayerId = player_id;
            this.ItemName = item_name;
            this.Quantity = quantity;
        }
    }

    public class GeneratorModels
    {
        public static GamePlayer GenerateGamePlayer()
        {
            var testPlayer = new Faker<GamePlayer>()
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.NickName, f => f.Internet.UserName())
                .RuleFor(u => u.Gold, 10000);

            return testPlayer.Generate();
        }
        public static List<User> GenerateUsers(int cnt)
        {
            var fruit = new[] { "apple", "banana", "orange", "strawberry", "kiwi" };
            var testOrders = new Faker<Order>()
                .StrictMode(true)
                .RuleFor(o => o.Id, f => Guid.NewGuid().ToString())
                .RuleFor(o => o.ItemName, f => f.PickRandom(fruit))
                .RuleFor(o => o.Quantity, f => f.Random.Number(1, 10));

            var testUsers = new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
                .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
                .RuleFor(u => u.UserName, (f, u) => f.Internet.UserName(u.FirstName, u.LastName))
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.Orders, f => testOrders.Generate(f.Random.Number(1, 3)));

            return testUsers.Generate(cnt);
        }

        public static User GenerateUserSingle()
        {
            var fruit = new[] { "apple", "banana", "orange", "strawberry", "kiwi" };
            var testOrders = new Faker<Order>()
                .StrictMode(true)
                .RuleFor(o => o.Id, f => Guid.NewGuid().ToString())
                .RuleFor(o => o.ItemName, f => f.PickRandom(fruit))
                .RuleFor(o => o.Quantity, f => f.Random.Number(1, 10));

            var testUsers = new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
                .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
                .RuleFor(u => u.UserName, (f, u) => f.Internet.UserName(u.FirstName, u.LastName))
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.Orders, f => testOrders.Generate(f.Random.Number(1, 3)));

            return testUsers.Generate();
        }
    }
}
