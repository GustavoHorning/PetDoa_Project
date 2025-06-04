export interface DonorReadDTO {
  id: number;
  name: string;
  email: string;
  oAuth_Provider: string | null;
  oAuth_ID: string | null;
  registration_Date: string;
  profilePictureUrl?: string | null;
}


export interface UpdateDonorProfileDto {
  Name: string;
}

export interface ChangePasswordDto {
  CurrentPassword: string;
  NewPassword: string;
  ConfirmNewPassword: string;
}