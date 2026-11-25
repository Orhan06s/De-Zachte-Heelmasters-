export function errorMessage(error) {
  if (!error.response) {
    return 'De server is niet bereikbaar. Probeer het later opnieuw.'
  }
  return error.response.data?.message ?? 'Er ging iets mis. Probeer het later opnieuw.'
}