export default function Avatar({ url, name, size = 48 }: { url?: string | null; name: string; size?: number }) {
  const initials = name.trim().split(/\s+/).map(w => w[0]).slice(0, 2).join('').toUpperCase();
  if (url) {
    return <img className="avatar" src={url} alt={name} width={size} height={size}
      style={{ width: size, height: size }} />;
  }
  return (
    <div className="avatar avatar-fallback" style={{ width: size, height: size, fontSize: size * 0.4 }}>
      {initials}
    </div>
  );
}
