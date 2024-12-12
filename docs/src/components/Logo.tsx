import { cn } from "../lib/utils";

interface LogoProps {
  className?: string;
}

export const Logo: React.FC<LogoProps> = ({ className = '' }) => {
  return (
    <svg
      className={cn("transition-colors duration-200", className)}
      width="48"
      height="48"
      viewBox="0 0 48 48"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        d="M24 4L8 12V24C8 34.8 14.9 44.9 24 47C33.1 44.9 40 34.8 40 24V12L24 4Z"
        className="fill-blue-50 dark:fill-blue-950"
        stroke="currentColor"
        strokeWidth="2"
      />
      <path
        d="M18 14H30V34H18V14Z"
        className="fill-white dark:fill-gray-800"
        stroke="currentColor"
        strokeWidth="1"
      />
      <path
        d="M20 18H28M20 22H28M20 26H24"
        stroke="currentColor"
        strokeWidth="1.5"
        strokeLinecap="round"
      />
      <path
        d="M32 32L28 36L26 34"
        className="stroke-green-500 dark:stroke-green-400"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
        fill="none"
      />
      <path
        d="M20 30H16V34H20V30Z"
        className="fill-blue-500 dark:fill-blue-400"
      />
      <path
        d="M18 30V28C18 27.4477 18.4477 27 19 27H21C21.5523 27 22 27.4477 22 28V30"
        className="stroke-blue-500 dark:stroke-blue-400"
        strokeWidth="1.5"
        fill="none"
      />
    </svg>
  );
};

export default Logo;
