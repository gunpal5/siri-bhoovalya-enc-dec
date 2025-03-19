import re
import os
from pathlib import Path

def extract_number_pairs(text):
    """
    Extract all occurrences of number pairs between parentheses.
    Format: (x,y) where x and y are integers
    
    Args:
        text (str): The input text to search in
        
    Returns:
        list: A list of tuples containing the number pairs
    """
    pattern = r'\((\d+),(\d+)\)'
    matches = re.findall(pattern, text)
    # Convert strings to integers
    return [(int(x), int(y)) for x, y in matches]

def extended_gcd(a, b):
    """Extended Euclidean Algorithm to find gcd and Bézout coefficients."""
    if a == 0:
        return (b, 0, 1)
    else:
        gcd, x, y = extended_gcd(b % a, a)
        return (gcd, y - (b // a) * x, x)

def mod_inverse(a, m):
    """Calculate the modular multiplicative inverse of a modulo m."""
    gcd, x, y = extended_gcd(a, m)
    if gcd != 1:
        raise ValueError(f"Modular inverse does not exist for {a} mod {m}")
    else:
        return x % m

def calculate_lagrange_polynomial(points, mod=27):
    """
    Calculate the Lagrange polynomial for the given points modulo mod.
    
    Args:
        points (list): List of (x, y) tuples where x is the index and y is the value
        mod (int): Modulo value (default: 27)
        
    Returns:
        list: Coefficients of the Lagrange polynomial
    """
    # Extract x and y values
    x_values = [p[0] % mod for p in points]
    y_values = [p[1] % mod for p in points]
    
    n = len(points)
    
    # Check if x values are distinct (required for Lagrange)
    if len(set(x_values)) != n:
        raise ValueError("All x values must be distinct for Lagrange interpolation")
    
    # Calculate the coefficients of the Lagrange polynomial
    result_poly = [0] * n
    
    for i in range(n):
        # Calculate the Lagrange basis polynomial for the i-th point
        numerator = 1
        # Product of (x - x_j) for j != i
        denominator = 1
        
        for j in range(n):
            if i != j:
                # For the numerator, we need to calculate the product of (x - x_j)
                # For the denominator, we need to calculate the product of (x_i - x_j)
                denominator = (denominator * (x_values[i] - x_values[j])) % mod
        
        # Calculate the modular inverse of the denominator
        denominator_inv = mod_inverse(denominator % mod, mod)
        
        # Calculate the coefficient for this term
        coefficient = (y_values[i] * denominator_inv) % mod
        
        # Calculate the polynomial (x - x_0)(x - x_1)...(x - x_{i-1})(x - x_{i+1})...(x - x_{n-1})
        temp_poly = [1]
        for j in range(n):
            if i != j:
                # Multiply by (x - x_j)
                temp_poly = multiply_poly_by_binomial(temp_poly, [-x_values[j], 1], mod)
        
        # Multiply by the coefficient
        temp_poly = [(coefficient * coef) % mod for coef in temp_poly]
        
        # Add to the result polynomial
        result_poly = add_polynomials(result_poly, temp_poly, mod)
    
    return result_poly

def multiply_poly_by_binomial(poly, binomial, mod):
    """
    Multiply a polynomial by a binomial (a + bx) modulo mod.
    
    Args:
        poly (list): Coefficients of the polynomial
        binomial (list): Coefficients of the binomial [a, b]
        mod (int): Modulo value
        
    Returns:
        list: Coefficients of the resulting polynomial
    """
    # binomial = [a, b] represents a + bx
    a, b = binomial
    
    # Multiply by b*x
    shifted = [0] + [(b * coef) % mod for coef in poly]
    
    # Multiply by a
    scaled = [(a * coef) % mod for coef in poly]
    
    # Add the results
    return add_polynomials(shifted, scaled, mod)

def add_polynomials(poly1, poly2, mod):
    """
    Add two polynomials modulo mod.
    
    Args:
        poly1, poly2 (list): Coefficients of the polynomials
        mod (int): Modulo value
        
    Returns:
        list: Coefficients of the resulting polynomial
    """
    # Ensure poly1 is the longer polynomial
    if len(poly2) > len(poly1):
        poly1, poly2 = poly2, poly1
    
    result = poly1.copy()
    
    # Add the coefficients
    for i in range(len(poly2)):
        result[i] = (result[i] + poly2[i]) % mod
    
    return result

def process_folder(folder_path, mod=27, sample_size=None):
    """
    Process all text files in a folder, extract number pairs, and calculate equations 
    for x and y values using indices.
    
    Args:
        folder_path (str): Path to the folder containing text files
        mod (int): Modulo value (default: 27)
        sample_size (int): Number of sample points to use for the polynomial (default: None, uses all)
        
    Returns:
        dict: Dictionary with filenames as keys and results as values
    """
    results = {}
    folder = Path(folder_path)
    
    # Check if the folder exists
    if not folder.is_dir():
        print(f"Error: '{folder_path}' is not a valid directory")
        return results
    
    # Get all text files in the folder
    txt_files = list(folder.glob("*.txt"))
    
    if not txt_files:
        print(f"No text files found in '{folder_path}'")
        return results
    
    # Process each text file
    for file_path in txt_files:
        file_results = {}
        try:
            with open(file_path, 'r', encoding='utf-8') as file:
                content = file.read()
                number_pairs = extract_number_pairs(content)
                
                if number_pairs:
                    # Take a sample if specified
                    if sample_size and len(number_pairs) > sample_size:
                        # Evenly spaced sampling
                        step = len(number_pairs) // sample_size
                        sampled_pairs = [number_pairs[i] for i in range(0, len(number_pairs), step)]
                        if len(sampled_pairs) > sample_size:
                            sampled_pairs = sampled_pairs[:sample_size]
                    else:
                        sampled_pairs = number_pairs
                    
                    # Use indices as x-coordinates instead of the original x-values
                    # This guarantees unique x-coordinates for Lagrange interpolation
                    index_x_pairs = [(i % mod, pair[0] % mod) for i, pair in enumerate(sampled_pairs)]
                    index_y_pairs = [(i % mod, pair[1] % mod) for i, pair in enumerate(sampled_pairs)]
                    
                    try:
                        # Calculate Lagrange polynomials for x and y separately
                        x_poly = calculate_lagrange_polynomial(index_x_pairs, mod)
                        y_poly = calculate_lagrange_polynomial(index_y_pairs, mod)
                        
                        file_results = {
                            'number_pairs': number_pairs,
                            'sampled_pairs': sampled_pairs if sample_size else None,
                            'x_polynomial': x_poly,
                            'y_polynomial': y_poly,
                            'num_pairs': len(number_pairs),
                            'num_sampled': len(sampled_pairs)
                        }
                    except ValueError as e:
                        file_results = {
                            'number_pairs': number_pairs,
                            'error': str(e),
                            'num_pairs': len(number_pairs)
                        }
                
                results[file_path.name] = file_results
        except Exception as e:
            print(f"Error processing file '{file_path.name}': {e}")
    
    return results

def format_polynomial(coefficients):
    """Format a polynomial for display from its coefficients."""
    if not coefficients or all(c == 0 for c in coefficients):
        return "0"
    
    terms = []
    
    for i, coef in enumerate(coefficients):
        if coef == 0:
            continue
            
        if i == 0:
            terms.append(str(coef))
        elif i == 1:
            if coef == 1:
                terms.append("x")
            else:
                terms.append(f"{coef}x")
        else:
            if coef == 1:
                terms.append(f"x^{i}")
            else:
                terms.append(f"{coef}x^{i}")
    
    return " + ".join(terms)

def display_and_save_results(all_results, folder_path):
    """Display results and offer to save them to a file."""
    if all_results:
        print("\n=== Results ===")
        for filename, result in all_results.items():
            print(f"\nFile: {filename}")
            
            if 'number_pairs' in result and result['number_pairs']:
                print(f"Found {result['num_pairs']} number pairs")
                print(f"First few pairs: {result['number_pairs'][:5]}...")
                
                if result.get('sampled_pairs'):
                    print(f"Used {result['num_sampled']} sampled pairs for polynomial calculation")
                
                if 'x_polynomial' in result:
                    print("\nLagrange Polynomial for X values (mod 27):")
                    poly_str = format_polynomial(result['x_polynomial'])
                    print(f"P_x(i) = {poly_str}")
                    print("(Where i is the index from 0 to n-1 mod 27)")
                
                if 'y_polynomial' in result:
                    print("\nLagrange Polynomial for Y values (mod 27):")
                    poly_str = format_polynomial(result['y_polynomial'])
                    print(f"P_y(i) = {poly_str}")
                    print("(Where i is the index from 0 to n-1 mod 27)")
                
                if 'error' in result:
                    print(f"\nError calculating Lagrange polynomial: {result['error']}")
            else:
                print("No valid number pairs found.")
                
        # Optionally save all results to a single output file
        save_option = input("\nDo you want to save all results to a file? (y/n): ").lower()
        if save_option == 'y':
            output_file = Path(folder_path) / "lagrange_polynomials_mod27.txt"
            
            with open(output_file, 'w', encoding='utf-8') as f:
                for filename, result in all_results.items():
                    f.write(f"File: {filename}\n")
                    
                    if 'number_pairs' in result and result['number_pairs']:
                        f.write(f"Found {result['num_pairs']} number pairs\n")
                        f.write(f"First few pairs: {result['number_pairs'][:5]}...\n")
                        
                        if result.get('sampled_pairs'):
                            f.write(f"Used {result['num_sampled']} sampled pairs for polynomial calculation\n")
                        
                        if 'x_polynomial' in result:
                            f.write("\nLagrange Polynomial for X values (mod 27):\n")
                            poly_str = format_polynomial(result['x_polynomial'])
                            f.write(f"P_x(i) = {poly_str}\n")
                            f.write("(Where i is the index from 0 to n-1 mod 27)\n")
                        
                        if 'y_polynomial' in result:
                            f.write("\nLagrange Polynomial for Y values (mod 27):\n")
                            poly_str = format_polynomial(result['y_polynomial'])
                            f.write(f"P_y(i) = {poly_str}\n")
                            f.write("(Where i is the index from 0 to n-1 mod 27)\n")
                        
                        if 'error' in result:
                            f.write(f"\nError calculating Lagrange polynomial: {result['error']}\n")
                    else:
                        f.write("No valid number pairs found.\n")
                    
                    f.write("\n" + "-" * 50 + "\n\n")
                    
            print(f"Results saved to: {output_file}")
    else:
        print("No results found in any files.")

def main():
    """Main entry point of the program."""
    print("=" * 60)
    print("Lagrange Polynomial Calculator for X and Y Values (mod 27)")
    print("=" * 60)
    print("This program extracts number pairs in the format (x,y) from text files")
    print("and calculates separate equations for x and y values mod 27.")
    print("Each pair is indexed from 0 to 728, and these indices are used as x-coordinates.")
    print("=" * 60)
    
    # Folder containing text files
    folder_path = input("Enter the folder path containing text files: ")
    
    # Ask user if they want to use a sample of points
    use_sampling = input("For large datasets, using a smaller sample can help.\nDo you want to use sampling? (y/n): ").lower() == 'y'
    
    sample_size = None
    if use_sampling:
        sample_input = input("Enter the number of points to sample (e.g., 26 for a polynomial of degree 25): ")
        try:
            sample_size = int(sample_input)
        except ValueError:
            print("Invalid input. Using all points.")
    
    # Process all text files in the folder
    all_results = process_folder(folder_path, sample_size=sample_size)
    
    # Display and save results
    display_and_save_results(all_results, folder_path)

if __name__ == "__main__":
    main()