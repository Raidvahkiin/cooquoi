"use client";

import { FC, useState } from "react";

const navLinks = [
	{ name: "Home", href: "/" },
	{ name: "Ingredients", href: "/ingredients" },
];

export const Topbar: FC = () => {
	const [menuOpen, setMenuOpen] = useState(false);

	return (
		<nav className="bg-white border-b border-gray-200 px-4 py-3 flex items-center justify-between">
			<div className="flex items-center">
				<span className="text-xl font-bold text-gray-900">Cooquoi</span>
			</div>
			<div className="md:hidden">
				<button
					type="button"
					className="inline-flex items-center justify-center p-2 rounded-md text-gray-700 hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-indigo-500"
					aria-controls="mobile-menu"
					aria-expanded={menuOpen}
					onClick={() => setMenuOpen((open) => !open)}
				>
					<span className="sr-only">Open main menu</span>
					{menuOpen ? (
						<svg
							className="h-6 w-6"
							fill="none"
							stroke="currentColor"
							viewBox="0 0 24 24"
							xmlns="http://www.w3.org/2000/svg"
						>
							<title>Close menu</title>
							<path
								strokeLinecap="round"
								strokeLinejoin="round"
								strokeWidth={2}
								d="M6 18L18 6M6 6l12 12"
							/>
						</svg>
					) : (
						<svg
							className="h-6 w-6"
							fill="none"
							stroke="currentColor"
							viewBox="0 0 24 24"
							xmlns="http://www.w3.org/2000/svg"
						>
							<title>Open menu</title>
							<path
								strokeLinecap="round"
								strokeLinejoin="round"
								strokeWidth={2}
								d="M4 6h16M4 12h16M4 18h16"
							/>
						</svg>
					)}
				</button>
			</div>
			<div className="hidden md:flex space-x-6">
				{navLinks.map((link) => (
					<a
						key={link.name}
						href={link.href}
						className="text-gray-700 hover:text-indigo-600 font-medium transition-colors"
					>
						{link.name}
					</a>
				))}
			</div>
			{/* Mobile menu */}
			{menuOpen && (
				<div className="absolute top-16 left-0 w-full bg-white border-b border-gray-200 md:hidden z-20">
					<div className="flex flex-col space-y-2 px-4 py-3">
						{navLinks.map((link) => (
							<a
								key={link.name}
								href={link.href}
								className="text-gray-700 hover:text-indigo-600 font-medium transition-colors"
								onClick={() => setMenuOpen(false)}
							>
								{link.name}
							</a>
						))}
					</div>
				</div>
			)}
		</nav>
	);
};
