using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using WatchLibrary.Models;
using System;
using System.Collections.Generic;

public class CartService
{
    private readonly IHttpContextAccessor _httpContextAccessor;  // Bruges til at få adgang til HTTP-konteksten og sessionen
    private const string CartSessionKey = "Cart";  // Nøgle for at gemme og hente indkøbskurven fra sessionen


    public CartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Hent indkøbskurven fra sessionen, deserialiser den til en liste af CartItem objekter
    public List<CartItem> GetCart()
    {
        // Hent sessionstrengen for kurven
        var cartJson = _httpContextAccessor.HttpContext.Session.GetString(CartSessionKey);

        // Hvis der er en gemt kurv, deserialiser den; ellers returner en tom liste
        return cartJson != null ? JsonConvert.DeserializeObject<List<CartItem>>(cartJson) : new List<CartItem>();
    }

    // Gem den aktuelle indkøbskurv i sessionen som en JSON-streng
    public void SaveCart(List<CartItem> cart)
    {
        // Serialiser listen af CartItem objekter og gem den i sessionen
        _httpContextAccessor.HttpContext.Session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));
    }

    // Tilføj et produkt til kurven
    public void AddToCart(CartItem item)
    {
        item.Validate(); // Valider produktet før det tilføjes til kurven

        var cart = GetCart();  // Hent den aktuelle indkøbskurv

        // Find et eksisterende produkt i kurven, som har samme WatchId
        var existingItem = cart.Find(c => c.WatchId == item.WatchId);

        if (existingItem != null)
        {
            // Hvis produktet allerede findes i kurven, øg mængden og den totale pris
            existingItem.Quantity += item.Quantity;
            existingItem.TotalPrice += item.TotalPrice;
        }
        else
        {
            // Hvis produktet ikke findes i kurven, tilføj det
            cart.Add(item);
        }

        // Gem den opdaterede kurv i sessionen
        SaveCart(cart);
    }

    
    public bool RemoveFromCart(int watchId)
    {
        var cart = GetCart();  // Hent den aktuelle indkøbskurv

        // Fjern alle produkter med det specifikke WatchId fra kurven og få antallet af fjernede produkter
        var removedCount = cart.RemoveAll(c => c.WatchId == watchId);

        // Gem den opdaterede kurv
        SaveCart(cart);

        // Returner om der blev fjernet et produkt (f.eks. om der var et produkt med det WatchId)
        return removedCount > 0;
    }

    // Tøm indkøbskurven
    public void ClearCart()
    {
        // Gem en tom liste som den nye indkøbskurv
        SaveCart(new List<CartItem>());
    }
}
