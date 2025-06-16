import { Component, OnInit } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AdminService, Product, ProductDto } from '../../../core/services/admin.service';

@Component({
  selector: 'app-product-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, CurrencyPipe],
  templateUrl: './product-management.component.html',
  styleUrls: ['./product-management.component.scss']
})
export class ProductManagementComponent implements OnInit {
  products: Product[] = [];
  isLoading = true;
  isModalOpen = false;
  isEditMode = false;
  currentProductId: number | null = null;
  productForm!: FormGroup;

  constructor(
    private adminService: AdminService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.loadProducts();
    this.productForm = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      price: [0, [Validators.required, Validators.min(0.01)]],
      imageUrl: ['', Validators.required],
      isActive: [true]
    });
  }

  loadProducts(): void {
    this.isLoading = true;
    this.adminService.getAllProducts().subscribe(data => {
      this.products = data;
      this.isLoading = false;
    });
  }

  openAddModal(): void {
    this.isEditMode = false;
    this.productForm.reset({ isActive: true });
    this.isModalOpen = true;
  }

  openEditModal(product: Product): void {
    this.isEditMode = true;
    this.currentProductId = product.id;
    this.productForm.setValue({
      name: product.name,
      description: product.description,
      price: product.price,
      imageUrl: product.imageUrl,
      isActive: product.isActive
    });
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
  }

  onSubmit(): void {
    if (this.productForm.invalid) return;

    const productDto: ProductDto = this.productForm.value;

    if (this.isEditMode && this.currentProductId) {
      this.adminService.updateProduct(this.currentProductId, productDto).subscribe(() => this.onSuccess());
    } else {
      this.adminService.createProduct(productDto).subscribe(() => this.onSuccess());
    }
  }
  
  onDeactivate(id: number): void {
    if (confirm('Tem certeza que deseja desativar este produto? Ele não aparecerá mais na lojinha.')) {
      this.adminService.deactivateProduct(id).subscribe(() => this.onSuccess());
    }
  }

  private onSuccess(): void {
    this.loadProducts();
    this.closeModal();
  }
}