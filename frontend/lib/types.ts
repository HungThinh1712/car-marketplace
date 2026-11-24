export type ListingStatus = "Available" | "Sold";

export type ListingSummary = {
  id: string;
  make: string;
  model: string;
  year: number;
  price: number;
  currencyCode: string;
  mileage: number;
  mileageUnit: string;
  location: string;
  status: ListingStatus;
  coverPhotoUrl: string;
  publishedAt: string;
};

export type ListingPhoto = {
  id: string;
  url: string;
  sortOrder: number;
};

export type Listing = ListingSummary & {
  description: string;
  sellerDisplayName: string;
  updatedAt: string;
  photos: ListingPhoto[];
};

export type ListingList = {
  items: ListingSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export type Seller = {
  id: string;
  displayName: string;
  email: string;
};

export type Inquiry = {
  id: string;
  name: string;
  email: string;
  phone?: string;
  message: string;
  submittedAt: string;
};

export type InquiryCreated = {
  id: string;
  listingId: string;
  make: string;
  model: string;
  year: number;
  submittedAt: string;
};

export type ApiProblem = {
  detail?: string;
  errors?: Record<string, string[]>;
};
