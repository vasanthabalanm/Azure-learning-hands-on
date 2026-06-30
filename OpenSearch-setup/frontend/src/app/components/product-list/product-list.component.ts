import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SearchHit, Product } from '../../models/product.model';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.scss'
})
export class ProductListComponent {
  @Input() hits: SearchHit<Product>[] = [];

  getHighlight(hit: SearchHit<Product>, field: string): string {
    if (hit.highlights && hit.highlights[field] && hit.highlights[field].length > 0) {
      return hit.highlights[field][0];
    }
    return this.getFieldValue(hit.source, field);
  }

  private getFieldValue(source: Product, field: string): string {
    switch (field) {
      case 'name':
        return source.name;
      case 'description':
        return source.description;
      default:
        return '';
    }
  }

  formatPrice(price: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(price);
  }

  getStockStatus(stock: number): { text: string; class: string } {
    if (stock === 0) {
      return { text: 'Out of Stock', class: 'out-of-stock' };
    } else if (stock < 10) {
      return { text: `Only ${stock} left`, class: 'low-stock' };
    }
    return { text: 'In Stock', class: 'in-stock' };
  }
}
