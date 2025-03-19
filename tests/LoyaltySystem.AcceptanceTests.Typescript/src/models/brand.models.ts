export interface CreateBrandRequest {
  name: string;
  category?: string;
  description?: string;
  logoUrl?: string;
  contact?: ContactInfo;
  address?: Address;
}

export interface BrandResponse {
  id: string;
  name: string;
  category?: string;
  description?: string;
  logoUrl?: string;
  contact?: ContactInfo;
  address?: Address;
  status: string;
  createdAt: string;
  updatedAt: string;
}

export interface ContactInfo {
  email?: string;
  phone?: string;
  website?: string;
}

export interface Address {
  street?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
} 