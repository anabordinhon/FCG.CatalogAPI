using FCG.Catalog.Domain.Common.Entities;
using FCG.Catalog.Domain.Games.Entities;
using FCG.Catalog.Domain.Promotions.Enum;
using FCG.Catalog.Domain.Promotions.ValueObjects;
//using FCG.Catalog.Domain.Users.Entities;

namespace FCG.Catalog.Domain.Promotions.Entities;

public class Promotion : BaseEntity
{
    private Promotion(string description, ValidityPeriod period, DiscountRule discountRule)
    {
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("A descrição é obrigatória.");

        Period = period;
        DiscountRule = discountRule;
        Description = description;
        Status = period.IsActive(DateTime.UtcNow)
            ? PromotionStatusEnum.Ativo
            : PromotionStatusEnum.Agendado;
    }
    private Promotion() { }
    public ICollection<Game> Games { get; set; } = [];
    public int UserId { get; set; }
    public Guid GameId { get; set; }
    public Guid PublicId { get; private set; } = Guid.NewGuid();
    public ValidityPeriod Period { get; private set; } = default!;
    public DiscountRule DiscountRule { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public PromotionStatusEnum Status { get; private set; }

    public static Promotion Create(string description, ValidityPeriod period, DiscountRule discountRule)
    {
        return new Promotion(description, period, discountRule);
    }

    public void Cancel()
    {
        if (Status == PromotionStatusEnum.Expirado)
        {
            throw new InvalidOperationException("Não é possível cancelar uma promoção expirada.");
        }
        Status = PromotionStatusEnum.Cancelado;
    }

    public void CheckVigency(DateTime now)
    {
        if (Status == PromotionStatusEnum.Cancelado || Status == PromotionStatusEnum.Expirado)
            return;

        if (Period.IsActive(now))
        {
            if (Status != PromotionStatusEnum.Ativo)
            {
                Status = PromotionStatusEnum.Ativo;
            }
        }
        else if (now > Period.EndDate)
        {
            Status = PromotionStatusEnum.Expirado;
        }
    }

}