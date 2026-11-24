using CarMarketplace.Api.Entities;
using Microting.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CarMarketplace.Api.Data;

/// <summary>EF Core context for seller accounts, listings, photos, and inquiries.</summary>
public sealed class MarketplaceDbContext : DbContext
{
    /// <summary>Creates the context.</summary>
    public MarketplaceDbContext(DbContextOptions<MarketplaceDbContext> options)
        : base(options)
    {
    }

    /// <summary>Seller accounts.</summary>
    public DbSet<SellerAccount> SellerAccounts => Set<SellerAccount>();

    /// <summary>Listings.</summary>
    public DbSet<Listing> Listings => Set<Listing>();

    /// <summary>Listing photos.</summary>
    public DbSet<ListingPhoto> ListingPhotos => Set<ListingPhoto>();

    /// <summary>Inquiries.</summary>
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();

    /// <summary>Configures the MySQL provider for server 8.4.</summary>
    public static void Configure(DbContextOptionsBuilder options, string connectionString)
    {
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0)));
    }

    /// <summary>Maps the marketplace tables, indexes, and check constraints.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SellerAccount>(entity =>
        {
            entity.ToTable("seller_accounts");
            entity.HasKey(account => account.Id);
            entity.Property(account => account.Id).HasColumnName("id").HasMaxLength(36).ValueGeneratedNever();
            entity.Property(account => account.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
            entity.Property(account => account.DisplayName).HasColumnName("display_name").HasMaxLength(80).IsRequired();
            entity.Property(account => account.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
            entity.Property(account => account.CreatedAt).HasColumnName("created_at").HasColumnType("datetime(6)");
            entity.Property(account => account.LastSignedInAt).HasColumnName("last_signed_in_at").HasColumnType("datetime(6)");
            entity.HasIndex(account => account.Email).IsUnique().HasDatabaseName("ux_seller_accounts_email");
        });

        modelBuilder.Entity<Listing>(entity =>
        {
            entity.ToTable("listings", table =>
            {
                table.HasCheckConstraint("ck_listings_status", "status in ('Available', 'Sold')");
                table.HasCheckConstraint("ck_listings_year", "year >= 1900");
                table.HasCheckConstraint("ck_listings_price", "price > 0 AND price <= 99999999.99");
                table.HasCheckConstraint("ck_listings_mileage", "mileage_km >= 0 AND mileage_km <= 2000000");
            });
            entity.HasKey(listing => listing.Id);
            entity.Property(listing => listing.Id).HasColumnName("id").HasMaxLength(36).ValueGeneratedNever();
            entity.Property(listing => listing.SellerAccountId).HasColumnName("seller_account_id").HasMaxLength(36).IsRequired();
            entity.Property(listing => listing.Make).HasColumnName("make").HasMaxLength(50).IsRequired();
            entity.Property(listing => listing.Model).HasColumnName("model").HasMaxLength(50).IsRequired();
            entity.Property(listing => listing.Year).HasColumnName("year");
            entity.Property(listing => listing.Price).HasColumnName("price").HasPrecision(10, 2);
            entity.Property(listing => listing.MileageKm).HasColumnName("mileage_km");
            entity.Property(listing => listing.Location).HasColumnName("location").HasMaxLength(80).IsRequired();
            entity.Property(listing => listing.Description).HasColumnName("description").HasMaxLength(2000).IsRequired();
            entity.Property(listing => listing.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16).IsRequired();
            entity.Property(listing => listing.PublishedAt).HasColumnName("published_at").HasColumnType("datetime(6)");
            entity.Property(listing => listing.UpdatedAt).HasColumnName("updated_at").HasColumnType("datetime(6)");
            entity.HasOne(listing => listing.SellerAccount)
                .WithMany(account => account.Listings)
                .HasForeignKey(listing => listing.SellerAccountId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(listing => listing.SellerAccountId).HasDatabaseName("ix_listings_seller_account_id");
            entity.HasIndex(listing => new { listing.Status, listing.PublishedAt, listing.Id })
                .IsDescending(false, true, true)
                .HasDatabaseName("ix_listings_status_published_at_id");
        });

        modelBuilder.Entity<ListingPhoto>(entity =>
        {
            entity.ToTable("listing_photos", table =>
            {
                table.HasCheckConstraint("ck_listing_photos_sort", "sort_order >= 0 AND sort_order <= 9");
                table.HasCheckConstraint("ck_listing_photos_size", "byte_size >= 1 AND byte_size <= 8388608");
            });
            entity.HasKey(photo => photo.Id);
            entity.Property(photo => photo.Id).HasColumnName("id").HasMaxLength(36).ValueGeneratedNever();
            entity.Property(photo => photo.ListingId).HasColumnName("listing_id").HasMaxLength(36).IsRequired();
            entity.Property(photo => photo.SortOrder).HasColumnName("sort_order");
            entity.Property(photo => photo.ContentType).HasColumnName("content_type").HasMaxLength(32).IsRequired();
            entity.Property(photo => photo.StoredFileName).HasColumnName("stored_file_name").HasMaxLength(64).IsRequired();
            entity.Property(photo => photo.ByteSize).HasColumnName("byte_size");
            entity.HasOne(photo => photo.Listing)
                .WithMany(listing => listing.Photos)
                .HasForeignKey(photo => photo.ListingId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(photo => new { photo.ListingId, photo.SortOrder })
                .IsUnique()
                .HasDatabaseName("ux_listing_photos_listing_sort");
        });

        modelBuilder.Entity<Inquiry>(entity =>
        {
            entity.ToTable("inquiries");
            entity.HasKey(inquiry => inquiry.Id);
            entity.Property(inquiry => inquiry.Id).HasColumnName("id").HasMaxLength(36).ValueGeneratedNever();
            entity.Property(inquiry => inquiry.ListingId).HasColumnName("listing_id").HasMaxLength(36).IsRequired();
            entity.Property(inquiry => inquiry.BuyerName).HasColumnName("buyer_name").HasMaxLength(80).IsRequired();
            entity.Property(inquiry => inquiry.BuyerEmail).HasColumnName("buyer_email").HasMaxLength(254).IsRequired();
            entity.Property(inquiry => inquiry.BuyerPhone).HasColumnName("buyer_phone").HasMaxLength(32);
            entity.Property(inquiry => inquiry.Message).HasColumnName("message").HasMaxLength(1000).IsRequired();
            entity.Property(inquiry => inquiry.SubmittedAt).HasColumnName("submitted_at").HasColumnType("datetime(6)");
            entity.HasOne(inquiry => inquiry.Listing)
                .WithMany(listing => listing.Inquiries)
                .HasForeignKey(inquiry => inquiry.ListingId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(inquiry => new { inquiry.ListingId, inquiry.SubmittedAt })
                .IsDescending(false, true)
                .HasDatabaseName("ix_inquiries_listing_submitted_at");
        });
    }
}
