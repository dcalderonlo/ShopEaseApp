using FluentValidation;

namespace ShopEaseApp.Api.Features.Catalog.Products;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Variants).NotEmpty().WithMessage("At least one variant is required.");
        RuleForEach(x => x.Variants).ChildRules(v =>
        {
            v.RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            v.RuleFor(x => x.Price).GreaterThan(0);
            v.RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        });
    }
}

public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CategoryId).GreaterThan(0);
    }
}
