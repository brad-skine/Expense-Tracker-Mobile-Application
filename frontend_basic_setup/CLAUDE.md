- Standalone components, lazy routes in app.routes.ts, pages in src/app/pages, shared in components.
- Data: services under src/app/services or beside the component; `shareReplay({bufferSize:1, refCount:false})`
  for shared streams; `TransactionService.triggerRefresh()` after any write.
- State: signals + `computed`; RxJS only at the HTTP boundary.
- Styling: SCSS per component, reuse tokens from styles.scss and layout.scss; bottom-nav items follow layout.html.
- Verify: `npx tsc --noEmit` then `npx ng build`. Android: `ng build && npx cap sync android`.
