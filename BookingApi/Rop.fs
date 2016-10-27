(* Code in this module is stolen from Scott Wlaschin's "Railway Oriented
   Programming" (http://fsharpforfunandprofit.com/posts/recipe-part2),
   although simplified for this particular purpose. *)

module Ploeh.Samples.Rop

// The two-track type
type Result<'TSuccess, 'TFailure> =
| Success of 'TSuccess
| Failure of 'TFailure

// Convert a switch function into a two-track function
let bind f x =
    match x with
    | Success s -> f s
    | Failure f -> Failure f

// Convert a one-track function into a two-track function
let map f x =
    match x with
    | Success s -> Success(f s)
    | Failure f -> Failure f