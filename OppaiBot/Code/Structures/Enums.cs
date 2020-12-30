using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum EmbedColor
{
    server,
    error,
    success,
}

public class ItemEntity
{
    public string name;
    public int price;
    public int quantity;
    public int maxQuantity;
    public string description;
    public bool requiresRealCash;
}