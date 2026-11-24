import Link from "next/link";

export function SiteFooter() {
  return (
    <footer className="site-footer">
      <div className="footer-container">
        <div>
          <p className="logo">
            Cars <span>for sale</span>
          </p>
          <p>Browse cars, publish a listing, and send an inquiry.</p>
        </div>
        <div className="footer-box">
          <h3>Pages</h3>
          <Link href="/">Home</Link>
          <Link href="/register">Register</Link>
          <Link href="/sign-in">Sign in</Link>
        </div>
      </div>
      <p className="copyright">Cars for sale</p>
    </footer>
  );
}
