import { Component, computed, inject, input } from '@angular/core';
import { CategoryService } from '../../data/category.service';
import { CATEGORY_ICONS } from '../../data/category-icons';

@Component({
  selector: 'app-category-icon',
  standalone: true,
  template: `
    <svg 
      [attr.width]="size()" 
      [attr.height]="size()" 
      viewBox="0 0 24 24" 
      fill="none" 
      [attr.stroke]="info().color" 
      stroke-width="2" 
      stroke-linecap="round" 
      stroke-linejoin="round"
    >
      <path [attr.d]="info().iconPath" />
    </svg>
  `,
  styles: [`
    :host {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      line-height: 0;
    }
  `]
})
export class CategoryIconComponent {
  private categoryService = inject(CategoryService);
  
  name = input.required<string>();
  size = input<number>(16);
  
  info = computed(() => {
    const style = this.categoryService.styleByName().get(this.name());
    return style || { color: '#94a3b8', iconPath: CATEGORY_ICONS['tag'] };
  });
}
